using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text.Json;

class User
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}

static class Leaderboard
{
    private static List<User> users;

    static Leaderboard()
    {
        LoadLeaderboard();
    }

    private static void LoadLeaderboard()
    {
        try
        {
            string json = File.ReadAllText("leaderboard.json");
            users = JsonSerializer.Deserialize<List<User>>(json);
        }
        catch (FileNotFoundException)
        {
            users = new List<User>();
        }
    }

    private static void SaveLeaderboard()
    {
        string json = JsonSerializer.Serialize(users);
        File.WriteAllText("leaderboard.json", json);
    }

    public static void AddUser(User user)
    {
        users.Add(user);
        SaveLeaderboard();
    }

    public static List<User> GetLeaderboard()
    {
        return users.OrderByDescending(u => u.CharactersPerMinute).ToList();
    }
}

class TypingTest
{
    private static string textToType;
    private static Stopwatch stopwatch;
    private static bool timerExpired;

    public static void StartTest()
    {
        Console.Write("Введите ваше имя: ");
        string name = Console.ReadLine();

        GenerateTextToType();

        Console.WriteLine("Введите следующий текст:");
        Console.WriteLine(textToType);

        Console.WriteLine("Нажмите Enter, чтобы начать печатать.");
        Console.ReadLine();

        Console.Clear();
        Console.WriteLine("Начните печатать:");

        stopwatch = Stopwatch.StartNew();
        timerExpired = false;

        Thread timerThread = new Thread(StartTimer);
        timerThread.Start();

        string typedText = Console.ReadLine();

        stopwatch.Stop();
        timerExpired = true;

        int charactersTyped = typedText.Length;
        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        double charactersPerMinute = charactersTyped / elapsedSeconds * 60;
        double charactersPerSecond = charactersTyped / elapsedSeconds;

        User user = new User
        {
            Name = name,
            CharactersPerMinute = (int)charactersPerMinute,
            CharactersPerSecond = (int)charactersPerSecond
        };

        Leaderboard.AddUser(user);

        Console.Clear();
        Console.WriteLine("Тест завершен!");
        Console.WriteLine("Ваша скорость печати: {0} символов в минуту", charactersPerMinute);
        Console.WriteLine("Ваша скорость печати: {0} символов в секунду", charactersPerSecond);

        DisplayLeaderboard();
    }

    private static void GenerateTextToType()
    {
        textToType = "Быстрые белки бегут по большому бульвару, берегущему блестящие березы.";
    }

    private static void StartTimer()
    {
        Thread.Sleep(60000);
        if (!timerExpired)
        {
            timerExpired = true;
            stopwatch.Stop();
            Console.Clear();
            Console.WriteLine("Время истекло!");
            Console.WriteLine("Вы не закончили печатать текст.");
            DisplayLeaderboard();
        }
    }

    private static void DisplayLeaderboard()
    {
        Console.WriteLine("Таблица лидеров:");
        List<User> leaderboard = Leaderboard.GetLeaderboard();
        foreach (User user in leaderboard)
        {
            Console.WriteLine("{0}: {1} символов в минуту, {2} символов в секунду", user.Name, user.CharactersPerMinute, user.CharactersPerSecond);
        }
    }
}

class Program
{
    static void Main()
    {
        while (true)
        {
            TypingTest.StartTest();
            Console.WriteLine("Нажмите Enter, чтобы начать новый тест, или любую другую клавишу, чтобы выйти.");
            if (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                break;
            }
        }
    }
}
