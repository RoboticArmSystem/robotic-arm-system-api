using System;
using System.ComponentModel.DataAnnotations;

namespace RoboticArmSystem.Core.Models
{
    [ValidateSelf]
    public class GetListCommonParam
    {
        public string sortKey { get; set; } = string.Empty;
        public string sortType { get; set; } = string.Empty;
        public int? page { get; set; }
        public int? rows { get; set; }

        class ValidateSelfAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var input = value as GetListCommonParam;

                if (input != null)
                {
                    if (!string.IsNullOrWhiteSpace(input.sortKey) != !string.IsNullOrWhiteSpace(input.sortType))
                        throw new Exception("sortKey 與 sortType 需同時傳入");

                    if (input.page.HasValue != input.rows.HasValue)
                        throw new Exception("page 與 rows 需同時傳入");
                }

                return ValidationResult.Success;
            }
        }
    }
}
