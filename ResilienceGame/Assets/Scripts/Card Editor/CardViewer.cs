using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardViewer : MonoBehaviour
{
    public FileBrowser filebrowser;
    public List<CardForEditor> cards = new List<CardForEditor>();


    public void OpenFile()
    {
        cards = LoadCsv(filebrowser.filePath);

        for(int i = 0; i < cards.Count; i++)
        {
            Debug.Log(cards[i].ToString());
        }
    }

    private List<CardForEditor> LoadCsv(string path)
    {
        List<CardForEditor> cards = new List<CardForEditor>();

        // Ensure the file exists
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return cards;
        }

        if (!IsValidCsvFile(path))
        {
            Debug.LogError("File is not correct: " + path);
            return cards;
        }

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] entries = lines[i].Split(',');

            // Ensure the row has enough entries
            if (entries.Length >= 14)
            {
                CardForEditor card = new CardForEditor
                {
                    team = entries[0],
                    title = entries[1],
                    cost = int.Parse(entries[2]),
                    image = entries[3],
                    description = entries[4],
                    impact = entries[5],
                    percent = int.Parse(entries[6]),
                    spreadChange = int.Parse(entries[7]),
                    duration = int.Parse(entries[8]),
                    delay = int.Parse(entries[9]),
                    targetCount = int.Parse(entries[10]),
                    targetType = entries[11],
                    cardCount = int.Parse(entries[12]),
                    type = entries[13]
                };

                cards.Add(card);
            }
        }

        return cards;
    }

    public static bool IsCsvFile(string path)
    {
        return string.Equals(Path.GetExtension(path), ".csv", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidCsvFile(string path)
    {
        // First, check if it's a .csv file based on the extension
        if (!IsCsvFile(path))
            return false;

        // Attempt to read the file to see if it can be processed as a CSV
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue; // Empty lines are okay

                    string[] entries = line.Split(',');

                    // This is a basic check. If there are no commas, it might not be a valid CSV.
                    // You can add more rules here, depending on the specifics of your expected CSV format.
                    if (entries.Length < 1)
                        return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading CSV file: {ex.Message}");
            return false;
        }
    }

}
