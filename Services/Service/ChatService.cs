using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Contract.Services.Interface;
using Microsoft.EntityFrameworkCore;
using ModelViews.AIModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ChatService : IChatService
    {
        private readonly GeminiService _geminiService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork db, GeminiService geminiService)
        {
            _unitOfWork = db;
            _geminiService = geminiService;
        }

        public async Task<string> ProcessUserMessageAsync(int userId, string message)
        {
            // Tìm hoặc tạo hội thoại
            var conversationRepo = _unitOfWork.GetRepository<Conversation>();
            var messageRepo = _unitOfWork.GetRepository<Message>();

            var conversation = await conversationRepo.Entities
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await conversationRepo.InsertAsync(conversation);
                await _unitOfWork.SaveAsync(); // cần save để có conversation.Id
            }

            // 2. Lưu message người dùng
            var userMessage = new Message
            {
                Role = "user",
                Content = message,
                Timestamp = DateTime.UtcNow,
                ConversationId = conversation.Id
            };
            await messageRepo.InsertAsync(userMessage);

            // 3. Chuẩn bị contents gửi lên Gemini
            var contents = new List<GeminiContent>();

            // 3.1 Prompt định hướng để trả lời ngắn gọn
            contents.Add(new GeminiContent
            {
                role = "user",
                parts = new List<GeminiPart>
                {
                    new GeminiPart
                    {
                        text = "Bạn là chuyên gia tư vấn sản phẩm skincare cho website bán hàng. Hãy trả lời ngắn gọn (dưới 3 dòng), thân thiện và dễ hiểu, không liệt kê quá nhiều sản phẩm."
                    }
                }
            });

            // 3.2 Các tin nhắn cũ
            var pastMessages = await messageRepo.Entities
                .Where(m => m.ConversationId == conversation.Id)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            contents.AddRange(pastMessages.Select(m => new GeminiContent
            {
                role = m.Role,
                parts = new List<GeminiPart>
                {
                    new GeminiPart { text = m.Content }
                }
            }));

            // 3.3 Tin nhắn hiện tại
            contents.Add(new GeminiContent
            {
                role = "user",
                parts = new List<GeminiPart>
                {
                    new GeminiPart { text = message }
                }
            });

            // 4. Gọi Gemini
            var aiResponse = await _geminiService.ChatWithHistoryAsync(contents);

            // 5. Lưu phản hồi từ AI
            var aiMessage = new Message
            {
                Role = "model",
                Content = aiResponse,
                Timestamp = DateTime.UtcNow,
                ConversationId = conversation.Id
            };
            await messageRepo.InsertAsync(aiMessage);

            await _unitOfWork.SaveAsync();

            return aiResponse;
        }
    }
}
