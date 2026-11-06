using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace SupportDesk.UnitTests;

public static class ModelValidation
{
    public static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    public static String[] ErrorsFor(this List<ValidationResult> results, string member) => results.Where(r => r.MemberNames.Contains(member)).Select(r => r.ErrorMessage ?? "").ToArray();

}
