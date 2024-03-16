using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace WariosWoodsRando
{
    public class Rando : MainWindow
    {
        /// <summary>
        /// Main randomizing logic function
        /// </summary>
        /// <param name="filePath">Vanilla rom path</param>
        /// <param name="inputSeed">Seed input taken from the textbox</param>
        public static void Main(string filePath, string inputSeed, MainWindow mw)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            //Level data
            long startOffset = 0x81AC;
            long endOffset = 0xA00F;

            int seed;
            
            string romHash = GetMD5Hash(filePath);
            Debug.WriteLine(romHash);

            //byte shift in the checksum for some roms (unsure of the reason) so I added 2.
            //Might have to change this method soon.

            //if (romHash != "d8cfbee2a988e6608d1a017923d937a8" && romHash != "fe84c1d05561e9cb9ba1acfa4a20ed8f")
            if (romHash != "e0875e4e768d48c90e58b74c110a9822")
            {
                MessageBox.Show(
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
            byte[] specificBytes = {0x00, 0x90, 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0};


            Console.WriteLine(randomizedFilePath);


            CopyFile(filePath, randomizedFilePath);
            ModifyFile(randomizedFilePath, startOffset, endOffset, specificBytes, seed, mw);

            MessageBox.Show(
            "ROM Randomized with success!\nIts located alongside your normal ROM.",
            "Success",
            MessageBoxButton.OK, MessageBoxImage.Information
                );
 
        }
        static bool IsSevenDigitNumber(int number) { return number >= 1000000 && number <= 9999999; }

        /// <summary>
        /// Creating an MD5 HASH
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static string GetMD5Hash(string filePath)
        {
            using (MD5 md5 = MD5.Create())
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Seek(0x07, SeekOrigin.Begin);
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
 

        static List<byte> HeightByteList(int count, byte minValue, byte maxValue, int seed)
        {
            if (count <= 0 || minValue > maxValue)
                throw new ArgumentException("Invalid parameters for byte list.");

            Random random = new Random(seed);
            List<byte> byteList = new List<byte>(count);

            for (int i = 0; i < count; i++)
            {
                byte randomValue = (byte)random.Next(minValue, maxValue + 1);
                byteList.Add(randomValue);
            }
            int totalBytes = 0;


            foreach (byte l in byteList)
            {
                totalBytes += l * 7;
            }
            Console.WriteLine(totalBytes.ToString());

            if (totalBytes < (7780 - count))
                return byteList;
            else 
            {
                Debug.WriteLine("bytes exceeded, rerolling...");
                Thread.Sleep(20);
                return HeightByteList(count, minValue, maxValue, seed);
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
         * Will do later
         */

        /*
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
                {
                    encodedBytes.Add(specialCharactersMapping[c]);
                }
                else
                {
                    encodedBytes.Add((byte)c);
                }

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
        */
        static void ModifyFile(string filePath, long startOffset, long endOffset, byte[] specificBytes, int seed, MainWindow mw)
        {

            /*
             * 206 is the number of layouts supposedly coded in the main area. 
             * Prior to this, the number was set 202 to avoid rerolling
             * (rerolling is necessary in case the number of lines exceeded the maximum amount of space)
             * Problem is due to how the game pick the levels, the last rounds in this list were kinda messed up
             * Causing invisible blocks and such, this should fix it.
             */

            List<byte> byteList = HeightByteList(206, 0x01, 0x09, seed);
            long roundsPointerStart = 0x8010;

            bool lastLayout = false;
            int final = 0;

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

                        //VANILLA HEIGHTS + RANDOM
                        if ((bool)mw.Box_vanilla_height.IsChecked)
                        {
                            if (fileData[offset] >= 0x01 && fileData[offset] <= 0x0F)
                            {
                                continue;
                            }
                            byte newByte = specificBytes[random.Next(specificBytes.Length)];
                            fileData[offset] = newByte;
                            continue;
                        }

                        //Preventing overflow and useless data usage
                        if (layout >= 206) {
                            if (layout == 206){ final = fileData[offset - 1] * 7; layout++; }

                            if (!lastLayout)
                            {
                                fileData[offset] = specificBytes[random.Next(specificBytes.Length)];
                                final--;
                                if(final == 0)
                                {
                                    lastLayout = true;
                                }
                            } else {
                                fileData[offset] = 0x00;
                            }
                            continue;
                        }
                        // FULL RANDOM
                        if(counter == 0 && (bool)!mw.Box_vanilla_height.IsChecked)
                        {
                            long pointerLocation = roundsPointerStart + layout * 2;

                            fileData[offset] = byteList[layout];
                            counter = byteList[layout]*7;

                            fileData[pointerLocation + 1] = (byte)((offset) - 0x10 >> 8); //first byte read
                            fileData[pointerLocation] = (byte)((byte)(offset) - 0x10); //second byte read 


                            /* 
                             * Debug stuff
                             * 
                            byte t1 = (byte)((offset) - 0x10 >> 8);
                            byte t2 = (byte)((byte)(offset) - 0x10);
                            Console.WriteLine("{0:X} {1:X}",t1, t2
                            */

                            layout++;

                        } else
                        {
                            fileData[offset] = specificBytes[random.Next(specificBytes.Length)];
                            byte test = fileData[offset];
                            counter--;
                        }
                         
                    }

                    fileStream.Seek(roundsPointerStart, SeekOrigin.Begin);
                    fileStream.Write(fileData, (int)roundsPointerStart, (int)(endOffset - roundsPointerStart + 1));
                }

                //Post rando edits
                List<byte> Bytes = new List<byte>();

                /////////////////////////////
                ////// Seed writing /////////
                /////////////////////////////
               
                //Locations of the 2 menus where the seed is gonna be written
                int menu1 = 0x28A1A;
                int menu2 = 0x292DD;


                // Digit to byte conversion
                while (seed > 0)
                {
                    byte digit = (byte)(seed % 10);
                    Bytes.Insert(0, digit);
                    seed /= 10;
                }
                //$E0 is where the tileset is located. So we add 0xE0 to each digit
                for (int i = 0; i < Bytes.Count; i++)
                    Bytes[i] += 0xE0;
                
                //Modify the following bytes
                EditBytes(filePath, menu1, Bytes.ToArray());
                EditBytes(filePath, menu2, Bytes.ToArray());

                /////////////////////////////
                ////// Music Stuff  /////////
                /////////////////////////////
                
                //Musics location ROM adresses
                int pointer = 0x38F9F;

                if ((bool)mw.Box_no_music.IsChecked)
                {
                    //0x4D0 = size of the music spot, we just nuke everything if no music enabled.
                    //outdated code will be replaced soon enough 
                    for (int i = 0; i < 0x4D0; i++)
                        Bytes.Insert(0, 0x00);
                } 

                EditBytes(filePath, pointer, Bytes.ToArray());

                byte[] music = {0x0};
                int musicPointer = 0x17038;
                int wariomusicPointer = 0x1703D;
                switch (mw.cbox_musics.SelectedIndex)
                {
                    case 0:
                        music[0] = 0x13;
                        break;
                    case 1:
                        music[0] = 0x0A;
                        break;
                    case 2:
                        music[0] = 0x10;
                        break;
                    case 3:
                        music[0] = 0x06;
                        break;
                }
                EditBytes(filePath, musicPointer, music);

                if ((bool)mw.Box_wario_music.IsChecked)
                {
                    music[0] = 0x16;
                    EditBytes(filePath, wariomusicPointer, music);
                }

                /////////////////////////
                /// ROUNDS PARAMETERS ///
                /////////////////////////
                
                startOffset = 0xAB5C;
                endOffset = 0xAE03;
                byte[] enemiesTypesEASY = { 0x00, 0x11, 0x10, 0x02, 0x03, 0x04, 0x05, 0x60, 0x07, 0x21};
                //byte[] enemiesTypesHARD = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
                byte[] speedTypes = { 0x01, 0x03, 0x07, 0x0F, 0x1F, 0x3F, 0x7F, 0xFF };

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    fileData = new byte[fileStream.Length];
                    fileStream.Read(fileData, 0, fileData.Length);


                    int counter = 1;
                    for (long offset = startOffset+1; offset <= endOffset; offset++)
                    {
                        Random random = new Random(seed+(int)offset);
                        if (counter == 3) 
                        {
                            switch(mw.Box_random_gimmicks.IsChecked)
                            {
                                case null:
                                    byte bit1 = (byte)random.Next(0, 8);
                                    byte bit2 = (byte)random.Next(0, 8);

                                    byte resultByte = (byte)((bit1 << 4) | bit2);


                                    //Temporary fix to avoid ?? AND double enemies to be together
                                    if(resultByte == 0x67 || resultByte == 0x76)
                                        resultByte = 0x66;

                                    fileData[offset] = resultByte;
                                    break;
                                case true:
                                    fileData[offset] = enemiesTypesEASY[random.Next(enemiesTypesEASY.Length)];
                                    break;
                                case false:
                                    break;
                            }
                        }

                        if(counter == 4 && (bool)mw.Box_random_speed.IsChecked)
                            fileData[offset] = speedTypes[random.Next(speedTypes.Length)];

                        if (counter == 7)
                            counter = 0;

                        counter++;
                    }

                    fileStream.Seek(startOffset, SeekOrigin.Begin);
                    fileStream.Write(fileData, (int)startOffset, (int)(endOffset - startOffset + 1));
                }

                Console.WriteLine("File modified successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message} {ex.Data}");
            }
        }

    }
}