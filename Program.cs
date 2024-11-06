using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FuncomDBFixGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Specify the file name (assuming it is in the same directory as the executable)
            string fileName = "ConanSandbox.log";
            string outputFileName = "GeneratedSql.sql";

            // Define the regex pattern to match to log entries in ConanSandbox.log
            //string pattern = @"\[\d+.\d+.\d+-\d+.\d+.\d+.\d+\].* NameToLoad: (.*)\n.*\n\[\d+.\d+.\d+-\d+.\d+.\d+.\d+\].* String asset reference \""None\"".*slow.";
            string pattern = @"(\[\d+.\d+.\d+-\d+.\d+.\d+.\d+\]).* NameToLoad: (.*)[\S\s]*? resolving it will be really slow.";
            
            try
            {
                // Check if the file exists in the same directory as the executable
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File '{fileName}' not found in the current directory.");
                    return;
                }

                // Read entire content of the file
                var lines = File.ReadAllLines(filePath).Reverse();
                string content = string.Join(Environment.NewLine, lines);
                //var content = File.ReadAllText(filePath);

                Console.WriteLine($"Searching for pattern '{pattern}' in '{fileName}'...");

                // Execute Regex pattern matching on the read contnet
                MatchCollection matches = Regex.Matches(content.ToString(), pattern);

                // Write matches to the output file
                string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

                if (File.Exists(outputFilePath))
                {
                    Console.WriteLine($"File '{outputFileName}' already exists and will be deleted.");
                    File.Delete(outputFilePath);
                }

                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (Match match in matches)
                    {
                        string target = match.Groups[2].ToString().Replace("\r", "");
                        writer.WriteLine($"DELETE FROM buildable_health WHERE object_id IN(SELECT DISTINCT object_id FROM buildings WHERE object_id IN (SELECT DISTINCT object_id FROM properties WHERE object_id IN (SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%'))));");
                        writer.WriteLine($"DELETE FROM buildings WHERE object_id IN(SELECT DISTINCT object_id FROM properties WHERE object_id IN (SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%')));");
                        writer.WriteLine($"DELETE FROM properties WHERE object_id IN(SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%'));");
                        writer.WriteLine($"DELETE FROM actor_position WHERE class LIKE '{target}%';");
                        writer.WriteLine("--");  // Separator between matches
                    }
                    writer.WriteLine("VACUUM;");
                    writer.WriteLine("REINDEX;");
                    writer.WriteLine("ANALYZE;");
                    writer.WriteLine("PRAGMA integrity_check;");
                }

                Console.WriteLine($"Matches used to generate script in file: '{outputFileName}' with '--' as a separator.");
                Console.WriteLine($"Hit Any Key to Close.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
