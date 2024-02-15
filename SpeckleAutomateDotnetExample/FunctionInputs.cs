using Nuke.Common;

using System.ComponentModel;

/// <summary>
/// This class describes the user specified variables that the function wants to work with.
/// </summary>
/// This class is used to generate a JSON Schema to ensure that the user provided values
/// are valid and match the required schema.
internal struct FunctionInputs
{
    [Secret]
    [Required]
    [DisplayName("SendGridHelper API Token")]
    [Description("You will need to have a SendGridHelper API Token")]
    public string SendGridAPIKey { get; set; }
}
