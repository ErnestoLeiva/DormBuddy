# 🤝 Contributor Setup Guide for DormBuddy

This guide will help you set up the project on your local machine using Visual Studio Code (VS Code). Follow these steps to get started:

## 🛠️ Prerequisites
Before you begin, make sure you have the following installed on your machine:
- [Git](https://git-scm.com/) (for version control)
- [Visual Studio Code](https://code.visualstudio.com/) (for editing and running the project)
- [MySQL](https://www.mysql.com/) (for database management)
- [C# and .NET SDK](https://dotnet.microsoft.com/download) (for backend development)

## 🍴 Step 1: Fork and Clone the Repository

1. Fork the [DormBuddy repository](https://github.com/ErnestoLeiva/DormBuddy.git) on GitHub by clicking the "Fork" button at the top right of the repository page.
2. After forking, open a terminal in Visual Studio Code or on your local machine, and clone your forked repository:
   ```bash
   git clone https://github.com/ErnestoLeiva/DormBuddy.git
3. Navigate into the project folder:
   ```bash
   cd C:/path/to/projectFolder/DormBuddy

## 🖥️ Step 2: Open the Project in VS Code
1. Launch **Visual Studio Code** and open the cloned project folder. You can do this by selecting **File > Open Folder...** and then choosing the <code>DormBuddy</code> folder.
2. Once the folder is open, VS Code will load all project files. You should now see the full project structure on the left sidebar.

## 🎥 Video showing step 1 and 2
[![How to clone repo in Visual Studio Code](https://img.youtube.com/vi/Qn-C4zrXCCQ/0.jpg)](https://www.youtube.com/watch?v=Qn-C4zrXCCQ)

## 🔧 Step 3: Install Extensions in VS Code
1. Ensure you have the following VS Code extensions installed:
   - **C# for Visual Studio Code**: This is required to work with C# code.
   - **MySQL**: A useful extension to interact with your MySQL database from VS Code.
   - **Live Server**: Helps in running your frontend HTML/JavaScript locally.
#### To install extensions, click on the Extensions icon on the left sidebar or press <code>Ctrl+Shift+X</code>, search for the extension, and click Install.

## 🗄️ Step 4: Setting Up the MySQL Database
1. Ensure **MySQL** is installed and running on your local machine.
2. Create a new database for the project using your preferred method (MySQL Workbench, command line, etc.).
   ```sql
   CREATE DATABASE dormbuddy_db;
3. Update the project’s database connection settings in the configuration file (e.g., <code>appsettings.json</code> or wherever the connection string is stored).
   ```json
   "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=dormbuddy_db;User=root;Password=yourpassword;"
    }

## 🏃‍♂️ Step 5: Running the Project
1. In VS Code, open a new terminal by selecting **Terminal > New Terminal** or pressing <code>Ctrl+</code>
2. Make sure all necessary packages are installed. If your project uses NuGet packages for C#, restore them by running:
   ```bash
   dotnet restore
3. Start the backend (C# server) by running:
   ```bash
   dotnet run
4. For the frontend, if you are using **Live Server**, right-click the <code>index.html</code> file in the **Explorer** on the left sidebar and choose **Open with Live Server** to run the frontend in your browser.
5. You should now see DormBuddy running locally! 🎉

## 🤝 Step 6: Making Contributions
1. Create a new branch for your feature or bugfix:
   ```bash
   git checkout -b feature-name
2. Make your changes in your branch.
3. Commit and push your changes:
   ```bash
   git add .
   git commit -m "Add your message here"
   git push origin feature-name
4. Submit a Pull Request on GitHub from your branch to the main repository.
##
Thank you for contributing to DormBuddy! 🎉
