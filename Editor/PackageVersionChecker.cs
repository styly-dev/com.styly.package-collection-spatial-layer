using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace STYLY.PackageCollection.Editor
{
    public class PackageVersionChecker : EditorWindow
    {
        private static ListRequest listRequest;
        private static Dictionary<string, string> currentDependencies;
        
        [MenuItem("STYLY/Check Package Updates")]
        public static void CheckPackageUpdates()
        {
            Debug.Log("Starting package version check...");
            LoadCurrentDependencies();
            CheckInstalledPackages();
        }
        
        private static void LoadCurrentDependencies()
        {
            currentDependencies = new Dictionary<string, string>();
            
            string packageJsonPath = Path.Combine(Application.dataPath, "..", "Packages", "com.styly.package-collection-spatial-layer", "package.json");
            
            if (!File.Exists(packageJsonPath))
            {
                Debug.LogWarning($"package.json not found at: {packageJsonPath}");
                return;
            }
            
            try
            {
                string jsonContent = File.ReadAllText(packageJsonPath);
                
                // Simple JSON parsing for dependencies section
                ParseDependenciesFromJson(jsonContent);
                
                Debug.Log($"Loaded {currentDependencies.Count} dependencies from package.json");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading package.json: {e.Message}");
            }
        }
        
        private static void ParseDependenciesFromJson(string jsonContent)
        {
            // Find the dependencies section
            int dependenciesStart = jsonContent.IndexOf("\"dependencies\"");
            if (dependenciesStart == -1) return;
            
            int braceStart = jsonContent.IndexOf('{', dependenciesStart);
            if (braceStart == -1) return;
            
            int braceCount = 1;
            int braceEnd = braceStart + 1;
            
            // Find the matching closing brace
            while (braceEnd < jsonContent.Length && braceCount > 0)
            {
                if (jsonContent[braceEnd] == '{') braceCount++;
                else if (jsonContent[braceEnd] == '}') braceCount--;
                braceEnd++;
            }
            
            if (braceCount > 0) return; // Invalid JSON
            
            string dependenciesSection = jsonContent.Substring(braceStart + 1, braceEnd - braceStart - 2);
            
            // Parse each dependency line
            string[] lines = dependenciesSection.Split(',');
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;
                
                // Parse "packageName": "version"
                string[] parts = trimmedLine.Split(':');
                if (parts.Length == 2)
                {
                    string packageName = parts[0].Trim().Trim('"');
                    string version = parts[1].Trim().Trim('"');
                    
                    if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(version))
                    {
                        currentDependencies[packageName] = version;
                    }
                }
            }
        }
        
        private static void CheckInstalledPackages()
        {
            listRequest = Client.List();
            EditorApplication.update += CheckListRequestProgress;
        }
        
        private static void CheckListRequestProgress()
        {
            if (listRequest.IsCompleted)
            {
                EditorApplication.update -= CheckListRequestProgress;
                
                if (listRequest.Status == StatusCode.Success)
                {
                    ProcessPackageList();
                }
                else
                {
                    Debug.LogError($"Failed to list packages: {listRequest.Error?.message}");
                }
            }
        }
        
        private static void ProcessPackageList()
        {
            Debug.Log("=== Package Version Check Results ===");
            
            var installedPackages = new Dictionary<string, string>();
            
            foreach (var package in listRequest.Result)
            {
                installedPackages[package.name] = package.version;
            }
            
            int updateCount = 0;
            int checkedCount = 0;
            
            foreach (var dependency in currentDependencies)
            {
                string packageName = dependency.Key;
                string expectedVersion = dependency.Value;
                
                checkedCount++;
                
                if (installedPackages.ContainsKey(packageName))
                {
                    string installedVersion = installedPackages[packageName];
                    
                    if (CompareVersions(installedVersion, expectedVersion) != 0)
                    {
                        updateCount++;
                        Debug.Log($"📦 {packageName}: Expected {expectedVersion}, Installed {installedVersion} " +
                                 $"{(CompareVersions(installedVersion, expectedVersion) > 0 ? "(newer)" : "(older)")}");
                    }
                    else
                    {
                        Debug.Log($"✅ {packageName}: {installedVersion} (up to date)");
                    }
                }
                else
                {
                    Debug.LogWarning($"❌ {packageName}: Expected {expectedVersion}, NOT INSTALLED");
                    updateCount++;
                }
            }
            
            Debug.Log($"=== Summary: {checkedCount} packages checked, {updateCount} discrepancies found ===");
            
            if (updateCount == 0)
            {
                Debug.Log("🎉 All packages are up to date!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {updateCount} packages have version discrepancies. Consider updating your project.");
            }
        }
        
        /// <summary>
        /// Simple semantic version comparison
        /// Returns: -1 if v1 < v2, 0 if v1 == v2, 1 if v1 > v2
        /// </summary>
        private static int CompareVersions(string v1, string v2)
        {
            try
            {
                // Remove any pre-release or build metadata for basic comparison
                v1 = v1.Split('-')[0].Split('+')[0];
                v2 = v2.Split('-')[0].Split('+')[0];
                
                var version1 = new Version(v1);
                var version2 = new Version(v2);
                
                return version1.CompareTo(version2);
            }
            catch
            {
                // Fallback to string comparison if version parsing fails
                return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}