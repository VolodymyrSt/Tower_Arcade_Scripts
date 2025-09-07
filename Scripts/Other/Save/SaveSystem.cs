using System;
using System.IO;
using UnityEngine;

namespace Game
{
    public class SaveSystem
    {
        private const string ENCRYPTION_DECRYPTION_WORD = "trytohackme";
        private readonly string _dataDirectoryPath;
        private readonly string _dataFileName;
        private readonly bool _useEncryption = true;

        public SaveSystem(string dataDirectoryPath, string dataFileName, bool useEncryption)
        {
            _dataDirectoryPath = dataDirectoryPath;
            _dataFileName = dataFileName;
            _useEncryption = useEncryption;
        }

        public SaveData Load()
        {
            string fullPath = Path.Combine(_dataDirectoryPath, _dataFileName);
            SaveData loadedData = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";

                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader rieder = new StreamReader(stream))
                            dataToLoad = rieder.ReadToEnd();
                    }

                    if (_useEncryption)
                        dataToLoad = EncryptDecrypt(dataToLoad);

                    loadedData = JsonUtility.FromJson<SaveData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return loadedData;
        }

        public void Save(SaveData gameData)
        {
            string fullPath = Path.Combine(_dataDirectoryPath, _dataFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var savedData = JsonUtility.ToJson(gameData, true);

            if (_useEncryption)
                savedData = EncryptDecrypt(savedData);

            try
            {
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                        writer.Write(savedData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string EncryptDecrypt(string data)
        {
            var key = ENCRYPTION_DECRYPTION_WORD;
            var result = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ key[i % key.Length]);
            }
            return new string(result);
        }
    }
}
