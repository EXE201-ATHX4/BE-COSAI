using Core.Store;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Base.BaseException;

namespace Core.Utils
{
    public class DateCompareGreaterThan: ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateCompareGreaterThan(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (DateTimeOffset?)value;
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
                throw new ErrorException((int)StatusCodeHelper.Notfound, "Not found", "Not found compare date data");

            var comparisonValue = (DateTimeOffset?)comparisonProperty.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue >= comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be less than {_comparisonProperty}.");
            }

            return ValidationResult.Success;
        }
    }
}
