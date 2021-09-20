using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace VeeamDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Veeam Demo app");

            try
            {
                Console.WriteLine("Enter the path to file:");
                string path = Console.ReadLine();
                Console.WriteLine("Enter the size of chunks in Kb (less or equal than 2 097 151: ");
                string numberStr = Console.ReadLine();
                int chunkSize = Int32.Parse(numberStr);
                SplitFile(path, chunkSize);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void SplitFile(string inputFile, int chunkSize)
        {
            try
            {
                int bufferSize = chunkSize * 1024;

                int index = 0;

                List<Thread> startedThreads = new List<Thread>();
                using (Stream input = File.OpenRead(inputFile))
                {
                    while (input.Position < input.Length)
                    {
                        byte[] buffer = new byte[bufferSize];

                        input.Read(buffer, 0, bufferSize);
                        var tempIndex = index++;

                        Thread th = new Thread(() => ComputeSha256Hash(tempIndex, buffer));
                        th.Start();
                        startedThreads.Add(th);
                    }
                }

                foreach (var th in startedThreads)
                {
                    th.Join();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void ComputeSha256Hash(int index, byte[] buffer)
        {
            try
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(buffer);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }

                    Console.WriteLine($"Chunk number {index} --- " + builder.ToString());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }
    }
}
