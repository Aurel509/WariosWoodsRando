using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace WariosWoodsRando
{
    public class Rando
    {
        /// <summary>
        /// Main randomizing logic function
        /// </summary>
        /// <param name="filePath">Vanilla rom path</param>
        /// <param name="inputSeed">Seed input taken from the textbox</param>
        public static void Main(string filePath, string inputSeed)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            //Level data
            long startOffset = 0x81AC;
            long endOffset = 0xA00F;

            int seed;
            
            string romHash = GetMD5Hash(filePath);
            Console.WriteLine(filePath);

            if (romHash != "d8cfbee2a988e6608d1a017923d937a8")
            {
                MessageBox.Show
                    (
                    "This is not a correct Wario's Woods NES ROM.", 
                    "Error",
                    MessageBoxButton.OK,MessageBoxImage.Error
                        );

                return;
            }

            if (int.TryParse(inputSeed, out int number) && IsSevenDigitNumber(number))
                seed = number;
            else
            {
                MessageBox.Show(
                "Invalid seed. Please make sure to enter a valid 7-digit number and retry.",
                "Error",
                MessageBoxButton.OK, MessageBoxImage.Error
                    );

                return;
            }
            string randomizedFilePath = folderPath + "\\woods" + "_" + seed + ".nes";

            //Enemy list. 0x00 being empty space 
            byte[] specificBytes = { 0x00, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0 };


            Console.WriteLine(randomizedFilePath);


            CopyFile(filePath, randomizedFilePath);
            ModifyFile(randomizedFilePath, startOffset, endOffset, specificBytes, seed);

            MessageBox.Show(
            "ROM Randomized with success!\nIts located alongside your normal ROM.",
            "Success",
            MessageBoxButton.OK, MessageBoxImage.Information
                );
 
        }
        static bool IsSevenDigitNumber(int number) { return number >= 1000000 && number <= 9999999; }

        static string GetMD5Hash(string filePath)
        {
            using (MD5 md5 = MD5.Create())
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] hashBytes = md5.ComputeHash(fileStream);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while calculating MD5 hash: {ex.Message}");
                    return string.Empty;
                }
            }
        }
        static void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
                Console.WriteLine("File copied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while copying the file: {ex.Message}");
            }
        }
 
        static List<byte> GenerateByteList(int count, byte minValue, byte maxValue)
        {
            if (count <= 0 || minValue > maxValue)
                throw new ArgumentException("Invalid parameters for generating byte list.");

            Random random = new Random();
            List<byte> byteList = new List<byte>(count);

            for (int i = 0; i < count; i++)
            {
                byte randomValue = (byte)random.Next(minValue, maxValue + 1);
                byteList.Add(randomValue);
            }
            int t = 0;


            foreach (byte l in byteList)
                t += l * 7;

            Console.WriteLine(t.ToString());

            if (t < 7779)
                return byteList;
            else 
            {
                Debug.WriteLine("bytes exceeded, rerolling...");
                Thread.Sleep(20);
                return GenerateByteList(count, minValue, maxValue);
            }
        }

        static void EditBytes(string filePath, int startBytePosition, byte[] newBytes)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    fileStream.Seek(startBytePosition, SeekOrigin.Begin);
                    fileStream.Write(newBytes, 0, newBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing bytes: {ex.Message}");
            }
        }

        /*
         * Unfinished yet. 
         */
        static string EncodeMessage(string input, int additionalValue)
        {
            int baseValue = 0x40;

            Dictionary<char, byte> specialCharactersMapping = new Dictionary<char, byte>
            {
                { '?', 0x3B },
                { '.', 0x3D },
                { '!', 0x5B },
                { ' ', 0xFF }
            };

            char[] characters = input.ToCharArray();
            List<byte> encodedBytes = new List<byte>();

            foreach (char c in characters)
            {
                if (char.IsLetter(c))
                {
                    int charValue = char.ToUpper(c) - 'A' + baseValue + additionalValue;
                    encodedBytes.Add((byte)charValue);
                }
                else if (specialCharactersMapping.ContainsKey(c))
                    encodedBytes.Add(specialCharactersMapping[c]);
                else
                    encodedBytes.Add((byte)c);
            }

            return BitConverter.ToString(encodedBytes.ToArray()).Replace("-", " ");
        }

        static void PickRandomMessage(int size)
        {
            Dictionary<string, int> messageList = new Dictionary<string, int>
            {
                { "SBDWOLF ISTHE GOAT     GO SUB AND FOLLOW HIM!! ", 2 },
                { "IM PRETTY POOR U KNOW. ANY GIFTERSPLEASE?      ", 2 },
                { "MAN THIS  GAME STINKS  WE SHOULD  PLAY THE SNES", 2 },
                { "GO FASTER!! PRISI IS ATTHE ENDGAMEALREADY!!    ", 2 }
            };

            foreach (var kvp in messageList)
            {
                string originalMessage = kvp.Key;
                int additionalValue = kvp.Value;

                string encodedMessage = EncodeMessage(originalMessage, additionalValue);
                Console.WriteLine($"text: {originalMessage}");
                Console.WriteLine($"encoded with bytes: {encodedMessage}\n");
            }
        }
        static void ModifyFile(string filePath, long startOffset, long endOffset, byte[] specificBytes, int seed)
        {
            List<byte> byteList = GenerateByteList(202, 0x01, 0x09);
            long roundsPointerStart = 0x8010;

            try
            {
                byte[] fileData;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    fileData = new byte[fileStream.Length];
                    fileStream.Read(fileData, 0, fileData.Length);

                    Random random = new Random(seed);
                    int layout = 0;
                    int counter = 0;
                    for (long offset = startOffset; offset <= endOffset; offset++)
                    {

                        //
                        /*if(layout == 150)
                        {
                            if (fileData[offset] >= 0x01 && fileData[offset] <= 0x0F)
                            {
                                continue;
                            }
                            byte newByte = specificBytes[random.Next(specificBytes.Length)];
                            fileData[offset] = newByte;
                            continue;
                        }*/
                        if(layout == 202)
                            //Avoid writing more code in case there are a few bytes left.
                            continue;
                        

                        if(counter == 0)
                        {
                            long pointerLocation = roundsPointerStart + layout * 2;

                            fileData[offset] = byteList[layout];
                            counter = byteList[layout]*7;

                            fileData[pointerLocation + 1] = (byte)((offset) - 0x10 >> 8); //first byte read
                            fileData[pointerLocation] = (byte)((byte)(offset) - 0x10); //second byte read 

                            /* 
                             * Debug stuff
                            byte t1 = (byte)((offset) - 0x10 >> 8);
                            byte t2 = (byte)((byte)(offset) - 0x10);
                            Console.WriteLine("{0:X} {1:X}",t1, t2
                            */

                            layout++;

                        } else
                        {
                            fileData[offset] = specificBytes[random.Next(specificBytes.Length)];
                            counter--;
                        }
                         
                    }

                    fileStream.Seek(roundsPointerStart, SeekOrigin.Begin);
                    fileStream.Write(fileData, (int)roundsPointerStart, (int)(endOffset - roundsPointerStart + 1));
                }

                //Writing the seed down in the game 
                int menu1 = 0x28A1A;
                int menu2 = 0x292DD;
                List<byte> seedBytes = new List<byte>();

                // Digit to byte conversion
                while (seed > 0)
                {
                    byte digit = (byte)(seed % 10);
                    seedBytes.Insert(0, digit);
                    seed /= 10;
                }
                //$E0 is where the tileset is located. So we add 0xE0 to each digit
                for (int i = 0; i < seedBytes.Count; i++)
                    seedBytes[i] += 0xE0;
                

                EditBytes(filePath, menu1, seedBytes.ToArray());
                EditBytes(filePath, menu2, seedBytes.ToArray());

                Console.WriteLine("File modified successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }

    }
}