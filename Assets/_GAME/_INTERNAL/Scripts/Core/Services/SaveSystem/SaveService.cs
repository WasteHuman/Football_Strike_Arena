using Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core.Services.SaveSystem
{
    public class SaveService
    {
        private PlayerData _playerData;
        private string SavePlayerDataFilePath => Path.Combine(Application.persistentDataPath, "player_data.sav");

        public PlayerData PlayerData => _playerData;

        public void Init(bool isDebug = false)
        {
            if(isDebug)
                DeleteAllSaves();

            LoadPlayerData();
        }

        private void LoadPlayerData()
        {
            if (File.Exists(SavePlayerDataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePlayerDataFilePath);
                    _playerData = JsonConvert.DeserializeObject<PlayerData>(json);

                    if (_playerData == null)
                    {
                        CreateNewPlayerData();
                        return;
                    }

                    _playerData.EnsureValidState();
                    Debug.Log("[SaveService] File loaded successfully.");
                }
                catch
                {
                    CreateNewPlayerData();
                }
            }
            else
                CreateNewPlayerData();
        }

        private void CreateNewPlayerData()
        {
            _playerData = new PlayerData();
            _playerData.EnsureValidState();
            SavePlayerData();
            Debug.Log("[SaveService] New player data created.");
        }

        public void SavePlayerData()
        {
            if (_playerData == null) 
                return;

            try
            {
                string json = JsonConvert.SerializeObject(_playerData);

                string tempPath = SavePlayerDataFilePath + ".tmp";
                File.WriteAllText(tempPath, json);

                if (File.Exists(SavePlayerDataFilePath))
                    File.Delete(SavePlayerDataFilePath);

                File.Move(tempPath, SavePlayerDataFilePath);

                Debug.Log("[SaveService] File saved successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] File save failed: {e.Message}");
            }
        }

        public void DeleteAllSaves()
        {
            if (File.Exists(SavePlayerDataFilePath))
                File.Delete(SavePlayerDataFilePath);

            Debug.Log("[SaveService] File deleted successfully.");
        }
    }
}