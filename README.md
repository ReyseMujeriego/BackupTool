# Projet_Genie_Logiciel  

## EasySave 🚀

Our team has recently joined the software publisher **ProSoft**. Under the responsibility of the **CIO**, we are tasked with managing the **EasySave** project, which involves developing backup software.  

As with all software in the **ProSoft Suite**, this software will be integrated into the company's pricing policy.  

- **Unit Price**: €200 excl. VAT 💶  
- **Annual Maintenance Contract (5 days/week, 8 AM - 5 PM, including updates)**: 12% of the purchase price (annual contract with tacit renewal and price adjustments based on the **SYNTEC index**).  

---

## Built With 🛠️

- [Visual Studio 2022](https://visualstudio.microsoft.com/fr/vs/)  
- [GitHub](https://github.com/)  
- [Draw.io](https://app.diagrams.net/)  

---

## Features 🌟

### **Version 1** 📜

The software is a **console application** developed using **.NET Core**. It supports the creation of up to **five backup jobs**.  

A job is defined by:  

- **Name** 🏷️  
- **Source folder path** 🗂️  
- **Target folder path** 📁  
- **Backup type**: Complete or Differential 🔄  

Once the user creates the jobs, they can execute them **individually** or **simultaneously**. The source and target paths can be located on **local, external, or network drives**.  

When backups are executed, the software must generate **two files**:

#### **Daily Log 📅**  

- Timestamp ⏰  
- Backup name 📝  
- Full source file path 🛣️  
- Full destination file path 🏠  
- File size 📏  
- File transfer time (in milliseconds) ⏳  

#### **Real-time Status 🔄**  

- Backup type 🗂️  
- Timestamp of the last action ⏱️  
- Backup job status (**Active, Inactive, etc.**) 🚦  

If the job is active:  
- Total number of eligible files 📂  
- Size of files to be transferred 💾  
- Progress:  
  - Number of remaining files 📝  
  - Size of remaining files 📊  
  - Full source file path being backed up 🔒  
  - Full destination file path 🔑  

---

### **Version 2** 🔧

#### **Graphical Interface 🖥️**
- The console mode is abandoned. The application must now be graphical and based on WPF or a framework of your choice (e.g., Avalonia).

#### **Unlimited Number of Jobs ♾️**
- The number of backup jobs is now unlimited.

#### **Encryption via CryptoSoft 🔐**
- The software must be able to encrypt files using CryptoSoft (developed during Project 4). Only files with extensions defined by the user in the general settings will be encrypted.

#### **Evolution of the Daily Log File 📈**
- The daily log file must include an additional piece of information:
  - Time required for file encryption (in ms)
    - `0`: no encryption 🚫
    - `>0`: encryption time (in ms) ⏳
    - `<0`: error code ❌

#### **Business Software 💼**
- If a business software is detected, the software must prevent the launch of a backup job. For sequential jobs, the software must finish backing up the current file.
- The user can define the business software in the general settings of the software. (Note: the calculator application can substitute for business software during demonstrations.)
- The stop must be logged in the log file 📝.

---

### **Version 3** 🚀

#### **Parallel Backup ⚡**
- Backup tasks are now executed in parallel, replacing the previous sequential mode.  

#### **Priority File Management 📂**
- Non-priority files cannot be backed up while priority extensions are still pending.  
- The priority extensions are predefined by the user in the general settings.  

#### **Large File Transfer Restrictions 🚫**
- To prevent network congestion, transferring two large files simultaneously is prohibited.  
- Threshold (n KB) is configurable.  
- ✅ While transferring a large file, other tasks can transfer smaller files** (as long as they comply with the priority rule).  

#### **Real-Time Task Interaction 🎛️** 
Users can interact with each backup task or all tasks at once:  
✅ Pause → Effective after the current file transfer.  
▶️ Play → Start or resume a paused task.  
⛔ Stop → Immediately halt the task and its ongoing transfer.  
📊 Live Progress Tracking → Display progress percentage in real time.  

#### **Auto-Pause for Business Applications ⏸️** 
- If a business software is detected running, all backup tasks will automatically pause.  
- Tasks resume automatically when the business software is closed.  
- Example: If the Calculator app is running, all backups must pause.  

#### **Remote Monitoring Console 🌐**   
- A graphical **remote interface (UI)** to monitor and manage backup tasks in real time.  
- Minimum requirements:  
  - 🖥️ Graphical Interface 
  - 📡 Communication via Sockets 

#### **Single-Instance Enforcement for CryptoSoft 🔒**
- CryptoSoft must run as a single instance.  
- It cannot be executed multiple times on the same machine.  
- Necessary modifications ensure proper single-instance management.  

#### **Adaptive Parallelism Based on Network Load (Optional) 📉**
- If network load exceeds a set **threshold**, the application will reduce parallel tasks to prevent saturation.  



## **Versions of EasySave** 📊

| VERSION  | STATUS | DOCUMENTATION 📚 |
|----------|--------|------------------|
| **Version 1** | [![DONE](https://img.shields.io/badge/DONE-%2328a745?style=flat-square&logo=github)](https://github.com/BlobJR/Projet_Genie_Logiciel/tree/main/Version%201.0/EasySave) | [User Documentation](https://github.com/BlobJR/Projet_Genie_Logiciel/blob/main/Version%201.0/DocUser-Version1.md) |
| **Version 2** | [![DONE](https://img.shields.io/badge/DONE-%2328a745?style=flat-square&logo=github)](https://github.com/BlobJR/Projet_Genie_Logiciel/tree/main/Version%201.0/EasySave) | [User Documentation](https://github.com/BlobJR/Projet_Genie_Logiciel/blob/main/Version%202.0/DocUser-Version2.md) |
| **Version 3** | [![DONE](https://img.shields.io/badge/DONE-%2328a745?style=flat-square&logo=github)](https://github.com/BlobJR/Projet_Genie_Logiciel/tree/main/Version%203.0/EasySave) | [User Documentation](https://github.com/BlobJR/Projet_Genie_Logiciel/blob/main/Version%203.0/DocUser-Version3.md) |

---

## **Authors** 👩‍💻👨‍💻

- [@BlobJR → Alexandre Roussel](https://github.com/BlobJR)  
- [@ReyseMujeriego → Alexandre Tellhez](https://github.com/ReyseMujeriego)  
- [@PaulineClausse](https://github.com/PaulineClausse)  
- [@BaHeCeJo → Baptiste Jousseaume](https://github.com/BaHeCeJo)  
