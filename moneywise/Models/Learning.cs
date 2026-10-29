namespace MoneyWise.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Situation { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string Category { get; set; } = "";
    public int MinAge { get; set; } = 10;
    public int XpReward { get; set; } = 20;
    public int Stage { get; set; } = 1;
    public List<QuizQuestion> Questions { get; set; } = new();
}

public class QuizQuestion
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public string Prompt { get; set; } = "";
    public string Explanation { get; set; } = "";
    public List<QuizOption> Options { get; set; } = new();
}

public class QuizOption
{
    public int Id { get; set; }
    public int QuizQuestionId { get; set; }
    public QuizQuestion? Question { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class QuizResult
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int CorrectCount { get; set; }
    public int Total { get; set; }
    public int XpEarned { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
