using System.Collections;
using System.Collections.Generic;

public class CardForEditor
{
    public string team { get; set; }
    public string title { get; set; }
    public int cost { get; set; }
    public string image { get; set; }
    public string description { get; set; }
    public string impact { get; set; }
    public int percent { get; set; }
    public int spreadChange { get; set; }
    public int duration { get; set; }
    public int delay { get; set; }
    public int targetCount { get; set; }
    public string targetType { get; set; }
    public int cardCount { get; set; }
    public string type { get; set; }

    public override string ToString()
    {
        return $"CardForEditor: {{\n" +
               $"\tTeam: {team},\n" +
               $"\tTitle: {title},\n" +
               $"\tCost: {cost},\n" +
               $"\tImage: {image},\n" +
               $"\tDescription: {description},\n" +
               $"\tImpact: {impact},\n" +
               $"\tPercent: {percent},\n" +
               $"\tSpreadChange: {spreadChange},\n" +
               $"\tDuration: {duration},\n" +
               $"\tDelay: {delay},\n" +
               $"\tTargetCount: {targetCount},\n" +
               $"\tTargetType: {targetType},\n" +
               $"\tCardCount: {cardCount},\n" +
               $"\tType: {type}\n" +
               "}";
    }
}
