# 📘 User Documentation - EasySave

## EasySave - User Guide

### 📝 Introduction
EasySave is a backup application that allows you to perform full or differential backups of files and folders. The application offers a simple graphical interface as well as advanced options such as file encryption and log management.

---

## 🖥 Installation
### Prerequisites:
- Windows 10 / 11
- .NET 8.0 installed
- Administrative permissions to access protected files

### Installation Steps:
1. Download the installation file `EasySave.zip`.
2. Extract the contents into a folder.
3. Run `EasySave.exe`.

---

## 🏠 User Interface
The main interface is divided into several sections:

- **Action Buttons**: Add, modify, delete, and execute backups.
- **Job List**: Displays all configured backups.
- **Console Log**: Shows real-time actions and possible errors.
- **Options**: Change the language, configure log format, manage encryption.

---

## 📂 Backup Management
### ➕ Add a Backup
1. Click on **"Add"**.
2. Fill in:
   - **Job Name**.
   - **Source Path** (folder to back up).
   - **Destination Path** (folder where the backup will be saved).
   - **Backup Type** (1 = full, 2 = differential).
3. Confirm.

### ✏ Modify a Backup
1. Select a job from the list.
2. Click on **"Modify"**.
3. Edit the necessary fields.
4. Confirm.

### ❌ Delete a Backup
1. Select a job from the list.
2. Click on **"Delete"**.
3. Confirm deletion.

---

## ▶ Executing Backups
### 🔹 Execute a Single Job
1. Select a job.
2. Click on **"Execute"**.

### 🔹 Execute Multiple Jobs Simultaneously
1. Select multiple jobs (**CTRL + Click**).
2. Click on **"Execute Selection"**.

---

## 🔒 Encryption and Decryption
### 🔹 Enable Encryption
1. Click on **"Encrypt"**.
2. Enter:
   - **Encryption Key** (password to protect files).
   - **File Extensions to Encrypt** (e.g., `.txt`, `.pdf`).
3. Confirm.

### 🔹 Decrypt Files
1. Click on **"Decrypt"**.
2. Choose:
   - **An existing job** (restore encrypted files from a backup).
   - **A specific path** (restore a specific folder).
3. Confirm.

---

## 📄 Log Management
Logs allow you to track actions performed by EasySave.

### 🔹 Change Log Format
- Click on **"JSON"** or **"XML"** depending on the desired format.

### 🔹 Access Logs
1. Open the log folder: `C:\ProgramData\EasySave\logs`
2. Open the file corresponding to the current date.

---

## 🔧 Advanced Options
### 🔹 Change Language
- Click on **"FR"** or **"EN"**.

### 🔹 Configure a Business Application
Some applications block backups to avoid conflicts:
1. Add the application to the configuration (`Settings.json`).
2. EasySave will pause the backup if the application is running.

---

## ❓ FAQ & Troubleshooting

### ❌ Common Issues

| Issue | Solution |
|----------|---------|
| Unable to add a job | Ensure all fields are correctly filled. |
| "Job not found" message | Check if the job name is correctly entered. |
| Files not backed up | Verify permissions and available disk space. |
| Encryption not working | Ensure the encryption key is correct. |

---

## 🛠 Technical Support

📧 **Email**: [support@easysave.com](mailto:support@easysave.com)  
📖 **Online Documentation**: [www.easysave.com/docs](https://www.easysave.com/docs)  
💬 **Forum**: [forum.easysave.com](https://forum.easysave.com)  

---

📌 **EasySave - Version 2.0**  
📝 **Last Update**: *2025-02-16*
