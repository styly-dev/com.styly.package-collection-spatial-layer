# STYLY Package Collection Editor Tools

This folder contains Unity Editor scripts for the STYLY Package Collection for Spatial Layer.

## PackageVersionChecker

### Purpose
The PackageVersionChecker helps maintain consistency between the package collection's declared dependencies and what's actually installed in Unity projects.

### Usage
1. In Unity Editor, go to the menu: `STYLY > Check Package Updates`
2. Check the Console window for results

### What it does
- Reads the dependencies from the package collection's `package.json`
- Lists all currently installed packages in the Unity project
- Compares expected versions with installed versions
- Reports any discrepancies in the Console

### Output Examples
```
✅ com.unity.probuilder: 6.0.5 (matches expected version)
📦 com.unity.inputsystem: Expected 1.14.0, Installed 1.13.0 (older than expected)
❌ com.styly.hands: Expected 0.0.1, NOT INSTALLED
```

### Summary Information
The tool provides a summary showing:
- Total dependencies checked
- Number of installed packages
- Number of version discrepancies

This helps developers ensure their Unity projects have the correct package versions as intended by the STYLY Package Collection.