<img src="Assets/Hero art.png" style="margin-bottom: 15px" />

# Widgets App

Boost your productivity and personalize your Windows desktop with powerful, beautiful widgets. Keep your essential information always visible — whether it's your schedule, tasks, weather, or quick notes — right on the desktop, always up to date and beautifully styled.

<a href="https://apps.microsoft.com/detail/9mw29r6qcr68">
    <img style="height: 50px;" src="Assets/ms-store-badge.svg" />
</a>

## Available Widgets

- 🕒 **Clock** — digital/analog time display with customizable format.  
- 📅 **Calendar** — monthly view with navigation, highlighting of the current date, **synchronized with events from Microsoft 365 Calendar**.  
- 💻 **CPU Monitor** — shows real-time CPU usage and load percentage.  
- 🎮 **GPU Monitor** — displays GPU load, memory usage, and temperature (if available).  
- 📝 **Notes** — simple sticky notes widget, **synchronized with tasks/notes from Microsoft 365 account**.  
- ✅ **To-Do List** — task manager with add/remove and check-off functionality, **synchronized with Microsoft To Do (Microsoft 365)**.  
- ☁️ **Weather** — shows current weather conditions and temperature using API data.  

## Technologies & Stack

- **Platform:** .NET 8 (Windows Desktop) + WinRT
- **UI Framework:** WPF (Windows Presentation Foundation)  
- **Architecture:** MVVM pattern for clean separation of UI and logic  
- **Data & Storage:** Local JSON for widget settings; integration with **Microsoft 365**
- **APIs:** Microsoft Graph API for Microsoft 365 synchronization; OpenWeatherMap API for Weather widget  
- **System Monitoring:** WMI and PerformanceCounter for CPU and GPU monitoring + LibreHardwareMonitor
- **Dependencies:** NuGet packages for JSON handling, API requests, and async operations  
- **Target OS:** Windows 10 / 11 
- **Minimum Build Version:** Windows 10.0.19041.0  
- **Windows on ARM supported**

## Warning

This repository is shared **for review purposes only**.  
It is **not intended for building or running the full project**.  
Some important parts of the project, including certain implementation files and secrets, have been intentionally removed.  

Do not attempt to use this code for production or distribution — it is provided solely for examination and feedback.

- ❌ No copying  
- ❌ No modifying  
- ❌ No redistribution  
- ❌ No commercial or non-commercial use  