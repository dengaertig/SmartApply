# JobScraper-CoverLetterGen

A C# console utility that scrapes job postings from StepStone, extracts key metadata (company, location, post date), and uses OpenAI’s GPT API to draft customized cover letters.

## 🚀 Features

- **Web scraping** with [HtmlAgilityPack]  
- **HTML parsing** to extract company name, location, posting date, and job title  
- **OpenAI GPT integration** to generate personalized cover‐letter drafts  
- **Configurable** via environment variables for API keys and target URLs  
- **Extensible**—easily adapt to other job boards or add e-mail notifications  

## 🛠️ Technologies & Topics

- **Language:** C#  
- **Libraries:** HtmlAgilityPack, RestSharp (or HttpClient), Newtonsoft.Json  
- **APIs:** OpenAI GPT  
- **GitHub Topics:**  
  `csharp` • `html-agility-pack` • `web-scraping` • `openai` • `job-scraper`

## 🔧 Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download) or later  
- A free [OpenAI API key](https://platform.openai.com/)  
- (Optional) Docker, if you’d like to containerize  

## ⚙️ Installation & Setup

1. **Clone the repo**  
   ```bash
   git clone https://github.com/your-username/JobScraper-CoverLetterGen.git
   cd JobScraper-CoverLetterGen
   ```

2. **Configure your environment**  
   Create a `.env` file in the project root:
   ```env
   OPENAI_API_KEY=sk-YOUR_OPENAI_KEY
   STEPSTONE_USERNAME=your.email@example.com
   STEPSTONE_PASSWORD=YourPassword123
   JOB_LISTING_URL=https://www.stepstone.de/jobs/werkstudent-informatik/in-berlin
   ```

3. **Restore & build**  
   ```bash
   dotnet restore
   dotnet build --configuration Release
   ```

## ▶️ Usage

Run the scraper and cover-letter generator in one command:
```bash
dotnet run --project src/JobScraper-CoverLetterGen
```

By default it will:
1. Fetch the target StepStone page
2. Parse the first 5 job postings
3. Print each company, location, post date, and a GPT-generated cover letter draft to the console

### Command-Line Options

```text
  --url <URL>             Override the default job-listing URL
  --max-jobs <number>     Limit to N postings (default: 5)
  --output <path>         Save results (JSON) to file
```

Example:
```bash
dotnet run --url https://www.stepstone.de/jobs/frontend-developer --max-jobs 3
```

## 📦 Docker

1. **Build**  
   ```bash
   docker build -t jobscraper .
   ```
2. **Run**  
   ```bash
   docker run --rm \
     -e OPENAI_API_KEY="$OPENAI_API_KEY" \
     -e JOB_LISTING_URL="https://www.stepstone.de/jobs/..." \
     jobscraper
   ```

## 📝 How It Works

1. **HTML fetch** via `HttpClient`  
2. **Parsing** with HtmlAgilityPack:  
   - Select each `<div data-genesis-element="BASE">` representing a job card  
   - Extract company name, location, and “online date” text  
3. **Prompt construction**:  
   ```text
   “Write me a concise cover letter for a Werkstudent Informatik position at {Company} in {Location}, posted {Date}…”  
   ```
4. **API call** to OpenAI GPT for each job  
5. **Output** to console or JSON file  

## 🤝 Contributing

1. Fork the repo  
2. Create your feature branch (`git checkout -b feature/your-feature`)  
3. Commit your changes (`git commit -m 'Add some feature'`)  
4. Push to the branch (`git push origin feature/your-feature`)  
5. Open a Pull Request  

## 🛡️ License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

*Happy scraping & good luck with your next application!*
