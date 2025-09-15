using DPBloom.Application.Base;
using TestOfTesting.Models.Enums;
using Type = System.Type;

namespace TestOfTesting.DTOs;

public class CreateQuestionDto
{
    public string Text { get; set; }
    public int Type { get; set; }
    public int CheckingType { get; set; }
    public int Category { get; set; }
    
    public List<CreateOptionDto> Options { get; set; }
}