using Core.Base;
using ModelViews.OrderModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelViews.AIModelViews;
namespace Contract.Services.Interface
{
    public interface IChatService
    {
        Task<string> ProcessUserMessageAsync(int userId, string message);
        Task<List<Chatresponse>> ChatHistoryAsync(int userId);
    }
}
