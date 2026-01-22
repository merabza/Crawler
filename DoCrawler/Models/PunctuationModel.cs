using ParametersManagement.LibParameters;

namespace DoCrawler.Models;

public sealed class PunctuationModel : ItemData
{
    public string? PctName { get; set; }
    public string? PctPunctuation { get; set; }
    public string? PctRegexPattern { get; set; }
    public int PctSortId { get; set; }
    public bool PctSentenceFinisher { get; set; }
    public bool PctCanBePartOfWord { get; set; }
}
