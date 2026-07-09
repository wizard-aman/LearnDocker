# EmployeeApp — DevOps Demo Project

ASP.NET Core 8 · Employee & Department CRUD · Docker · Swagger · GitHub Actions CI/CD · Azure

---

## What This Project Teaches

```
Code likhna    → .NET, EF Core, Controllers, DTOs
Test likhna    → xUnit, FluentAssertions, Integration Tests
Containerize   → Dockerfile, .dockerignore, multi-stage build
CI/CD          → GitHub Actions YAML, jobs, steps, secrets
Deploy         → GHCR (free registry), Azure App Service
```

---

## Project Structure

```
EmployeeApp/
├── .github/
│   └── workflows/
│       └── ci-cd.yml          ← GitHub Actions pipeline (most important file)
│
├── EmployeeApp/               ← Main API project
│   ├── Controllers/
│   │   ├── DepartmentsController.cs
│   │   └── EmployeesController.cs
│   ├── Data/
│   │   └── AppDbContext.cs    ← EF Core + seed data
│   ├── Models/
│   │   └── Models.cs          ← Entities + DTOs
│   ├── Dockerfile             ← Docker image banane ki recipe
│   ├── .dockerignore          ← Docker build se exclude karo
│   ├── Program.cs             ← App entry point + Swagger setup
│   └── appsettings.json
│
├── EmployeeApp.Tests/         ← Test project
│   ├── DepartmentTests.cs     ← 8 department tests
│   └── EmployeeTests.cs       ← 9 employee tests
│
└── EmployeeApp.sln            ← Solution file (dono projects tie karta hai)
```

---

## APIs

### Swagger UI
App chalane ke baad: **http://localhost:8080**
Swagger automatically khulta hai — browser se directly test karo.

### Departments — http://localhost:8080/api/departments

| Method | URL | Body | Description |
|--------|-----|------|-------------|
| GET | /api/departments | — | Sab departments + employee count |
| GET | /api/departments/{id} | — | Ek department |
| POST | /api/departments | `{"name":"IT","location":"Pune"}` | Naya banao |
| PUT | /api/departments/{id} | same as POST | Update karo |
| DELETE | /api/departments/{id} | — | Delete karo |

### Employees — http://localhost:8080/api/employees

| Method | URL | Body | Description |
|--------|-----|------|-------------|
| GET | /api/employees | — | Sab employees + department naam |
| GET | /api/employees/{id} | — | Ek employee |
| GET | /api/employees/by-department/{deptId} | — | Department ke employees |
| POST | /api/employees | see below | Naya banao |
| PUT | /api/employees/{id} | same as POST | Update karo |
| DELETE | /api/employees/{id} | — | Delete karo |

**Create Employee body:**
```json
{
  "name": "Aman Sharma",
  "email": "aman@company.com",
  "position": "Sr. Developer",
  "salary": 85000,
  "departmentId": 1
}
```

---

## How to Run

### Option 1: Docker (recommended — kuch install nahi chahiye)
```bash
# Build karo
docker build -t employeeapp ./EmployeeApp

# Run karo
docker run -d -p 8080:8080 --name employeeapp employeeapp

# Swagger UI: http://localhost:8080
# Health:     http://localhost:8080/health

# Logs dekho
docker logs employeeapp

# Stop karo
docker stop employeeapp && docker rm employeeapp
```

### Option 2: dotnet run
```bash
cd EmployeeApp
dotnet run
# http://localhost:8080
```

### Option 3: Tests run karo
```bash
dotnet test EmployeeApp.Tests/ --verbosity normal
# 17 tests hain — sab green hone chahiye
```

---

# DEVOPS DEEP UNDERSTANDING

Ye section interview ke liye sabse important hai.

---

## 1. Docker — Concepts Explained

### Dockerfile kya karta hai?

```
Dockerfile → docker build → Image → docker run → Container
```

```dockerfile
# Ye line kya karti hai?
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Microsoft ka official .NET SDK image download karo
# "AS build" matlab is stage ka naam "build" hai (multi-stage)

WORKDIR /src
# Container ke andar /src folder banao aur wahan jao
# Tumhare PC pe kuch nahi hota — ye sab container ke andar hai

COPY EmployeeApp/EmployeeApp.csproj EmployeeApp/
RUN dotnet restore EmployeeApp/EmployeeApp.csproj
# TRICK: pehle sirf .csproj copy karo, restore karo
# Kyun? Docker LAYER CACHE use karta hai
# Agar .csproj nahi badla → restore layer cached hai → skip hoga → FASTER BUILD

COPY EmployeeApp/ EmployeeApp/
RUN dotnet publish ... -o /app/publish
# Ab saara code copy karo aur compile karo

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# NAYI IMAGE start — SDK nahi, sirf runtime (200MB vs 800MB)
COPY --from=build /app/publish .
# Sirf compiled output copy karo "build" stage se
# Source code final image mein NAHI jaata — security + size
```

### Why multi-stage build?

```
Without multi-stage:  ~800MB image (SDK + source code + compiled output)
With multi-stage:     ~200MB image (sirf compiled output + runtime)

Benefits:
→ Faster docker pull on server (chhoti image)
→ Less attack surface (no SDK, no source code in production image)
→ Less storage cost
```

### Layer Cache — Docker ka Magic

```
docker build karne pe ye hota hai:

Step 1: FROM ... → cached (same image) ✓ SKIP
Step 2: WORKDIR  → cached ✓ SKIP
Step 3: COPY .csproj + restore → cached (csproj nahi badla) ✓ SKIP
Step 4: COPY source code → CHANGED (code badla) → REBUILD from here
Step 5: dotnet publish → REBUILD

Matlab sirf changed layers rebuild hote hain — FAST!
```

### .dockerignore kyun zaroori hai?

```
Bina .dockerignore ke:
  docker build context = poora folder (bin/, obj/, .git/, node_modules sab)
  = Slow upload, badi image

.dockerignore ke saath:
  docker build context = sirf zaroori files
  = Fast build, chhoti image
```

### Important Docker Commands

```bash
# Image banao
docker build -t myapp:1.0 ./EmployeeApp
#            -t = tag (naam:version)
#            ./EmployeeApp = Dockerfile ki location

# Container chalao
docker run -d -p 8080:8080 --name myapp myapp:1.0
#          -d = detached (background mein)
#          -p 8080:8080 = host:container port mapping
#          --name = container ko naam do

# Running containers dekho
docker ps

# Sab containers (stopped bhi)
docker ps -a

# Logs dekho
docker logs myapp
docker logs -f myapp   # follow = real time

# Container ke andar jao (debugging ke liye)
docker exec -it myapp bash

# Image registry pe push karo
docker tag myapp:1.0 ghcr.io/username/myapp:1.0
docker push ghcr.io/username/myapp:1.0

# Cleanup
docker stop myapp
docker rm myapp
docker rmi myapp:1.0
docker system prune -a   # sab unused images/containers delete
```

---

## 2. GitHub Actions — YAML Explained

### YAML file ki anatomy

```yaml
name: Pipeline ka naam (GitHub UI mein dikhta hai)

on:                        # TRIGGER — kab chale
  push:
    branches: [main]       # main pe push hone par
  pull_request:
    branches: [main]       # main pe PR open hone par

env:                       # GLOBAL VARIABLES — sabhi jobs mein available
  MY_VAR: "hello"

jobs:                      # JOBS — parallel ya sequential kaam
  job-1:                   # Job ka naam (koi bhi daal sakte ho)
    runs-on: ubuntu-latest # Kaun si machine pe chalao
    needs: []              # Koi dependency nahi — parallel chalega
    steps:                 # Steps — sequential chalte hain

      - name: Step naam    # GitHub UI mein dikhta hai
        uses: actions/checkout@v4   # Ready-made action use karo

      - name: Custom command
        run: echo "Hello"  # Direct terminal command

  job-2:
    needs: job-1           # Job 1 pass ke baad hi chalega (sequential)
    if: github.ref == 'refs/heads/main'   # Condition — sirf main pe
```

### Secrets kya hain aur kyun chahiye?

```
Problem: Pipeline mein Azure password, Docker credentials directly likhna unsafe hai
         Code GitHub pe public ho sakta hai — sab dekh lenge

Solution: GitHub Secrets
  → Encrypted values store karte hain
  → YAML mein ${{ secrets.MY_SECRET }} se access karo
  → GitHub UI mein bhi nahi dikhta — *** show hota hai
  → Sirf pipeline run ke time decrypt hota hai

Kahan set karo?
  GitHub repo → Settings → Secrets and variables → Actions → New secret
```

### GITHUB_TOKEN — Special Secret

```
Ye secret automatically available hota hai — set karna nahi padta
GitHub khud generate karta hai har pipeline run ke liye

Uses:
  → GHCR (GitHub Container Registry) pe login
  → GitHub API calls
  → Permissions: contents:read, packages:write, etc.

Expire: Job khatam hote hi expire ho jaata hai — secure
```

### github context variables

```yaml
${{ github.sha }}              # Commit hash: abc1234def
${{ github.ref }}              # Branch: refs/heads/main
${{ github.actor }}            # Pushkarne wala: wizard-aka-aman
${{ github.repository }}       # Repo: wizard-aka-aman/EmployeeApp
${{ github.repository_owner }} # Owner: wizard-aka-aman
${{ github.run_number }}       # Pipeline run number: 42
${{ github.event_name }}       # Event: push, pull_request
```

### Jobs — Parallel vs Sequential

```
Parallel (default — needs nahi diya):
  ┌─────────┐  ┌─────────┐
  │  job-1  │  │  job-2  │   ← dono ek saath chalte hain (fast)
  └─────────┘  └─────────┘

Sequential (needs use karke):
  ┌─────────┐
  │  job-1  │
  └────┬────┘
       │ pass hone ke baad
  ┌────▼────┐
  │  job-2  │
  └─────────┘

Hamare pipeline mein:
  build-and-test (har push pe)
       ↓ pass hone ke baad
  docker-build-push (sirf main pe)
       ↓ pass hone ke baad
  deploy (sirf main pe)
```

### Artifacts kya hain?

```
Problem: Job 2 mein banaya image tag, Job 3 mein chahiye
         Lekin har job alag runner pe chalta hai — files share nahi hoti

Solution: Artifacts
  Job 2: image-tag.txt file banao → upload-artifact se GitHub pe save karo
  Job 3: download-artifact se wahi file lo

Artifacts GitHub UI mein bhi dikhte hain — download kar sakte ho
Retention: configurable (humne 30 days rakha)
```

---

## 3. CI/CD — Full Flow Explained

### CI — Continuous Integration

```
"Integration" = code milana
"Continuous" = har baar jab koi code push kare

CI ka kaam:
1. Code download karo (checkout)
2. Dependencies install karo (dotnet restore)
3. Code compile karo (dotnet build)
4. Tests chalao (dotnet test)
5. Pass/Fail report karo

Agar tests fail ho to:
→ Pipeline red ho jaata hai
→ Developer ko email/notification milti hai
→ Code merge block ho jaata hai (branch protection rules se)
→ Broken code production mein nahi jaata
```

### CD — Continuous Deployment

```
"Deployment" = server pe code daalna
"Continuous" = automatically

CD ka kaam (CI pass ke baad):
1. Docker image banao
2. Registry pe push karo (GHCR)
3. Server ko batao — naya image use karo
4. Health check karo

Result: git push → automatically live on production
```

### Why separate CI and CD?

```
CI har jagah chale:
→ main pe push  ✓
→ develop pe push  ✓
→ PR open karo  ✓
Kisi bhi branch pe code break nahi hona chahiye

CD sirf main pe chale:
→ main pe push  ✓
→ develop pe push  ✗
→ PR  ✗
Sirf reviewed + approved code production pe jaaye
```

### Branch Protection — Broken code merge rokna

```
GitHub repo → Settings → Branches → Add rule → main

Tick karo:
✓ Require status checks to pass before merging
  → "build-and-test" job select karo
✓ Require branches to be up to date before merging
✓ Require pull request reviews before merging

Ab koi directly main pe push nahi kar sakta
PR karo → CI pass ho → Review ho → Merge hoga
```

---

## 4. GitHub Container Registry (GHCR)

### Docker Hub vs GHCR

```
Docker Hub:
  → Free tier mein limited pulls (100/6hr)
  → Private images ke liye payment
  → Alag account manage karna

GHCR (GitHub Container Registry):
  → GitHub account se hi use karo — alag account nahi
  → GitHub repo ke saath integrated
  → Private images FREE (GitHub account ke saath)
  → GITHUB_TOKEN se login — koi extra secret nahi
  → Image URL: ghcr.io/username/imagename:tag
```

### GHCR pe image kaise jaati hai

```yaml
# Login
- uses: docker/login-action@v3
  with:
    registry: ghcr.io
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}  # auto-generated!

# Push
- uses: docker/build-push-action@v5
  with:
    push: true
    tags: ghcr.io/username/employeeapp:latest
```

GHCR pe jaane ke baad:
→ GitHub repo → Packages tab mein dikhega
→ ghcr.io/username/employeeapp URL se pull kar sakte hain

---

## 5. Environments and Approvals

```
GitHub → Settings → Environments → New environment → "production"

Add required reviewers:
→ Deploy job chalne se pehle specified person ko approve karna hoga
→ Email aayegi "Deployment waiting for your approval"
→ Approve karo → deploy chale
→ Reject karo → deploy cancel

Ye Continuous Delivery vs Continuous Deployment ka difference hai:
  Continuous Delivery   = deploy-ready package automatically banta hai,
                          but production push ke liye manual approval
  Continuous Deployment = fully automatic, koi manual step nahi
```

---

## 6. Integration Tests — How They Work

```csharp
// WebApplicationFactory kya karta hai?
// Real ASP.NET Core app memory mein start karta hai
// Actual HTTP calls karo — HttpClient se
// Koi mock nahi — real controllers, real DB (InMemory), real middleware

public class DepartmentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DepartmentTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        // Ye client real HTTP calls karta hai in-memory app pe
    }

    [Fact]
    public async Task Create_ValidDepartment_Returns201()
    {
        // Arrange — data ready karo
        var dto = new CreateDepartmentDto("Marketing", "Bangalore");

        // Act — actual API call
        var response = await _client.PostAsJsonAsync("/api/departments", dto);

        // Assert — check karo
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        // FluentAssertions: .Should().Be() — readable assertions
    }
}
```

### Unit Test vs Integration Test

```
Unit Test:
→ Ek function/method test karta hai
→ Database, HTTP — sab mock
→ Fast (milliseconds)
→ Isolated

Integration Test (hamara approach):
→ Poori request lifecycle test karta hai
→ Controller → Service → DB — sab real
→ Slower but more realistic
→ Actually ensure karta hai ki sab milke kaam karta hai
```

---

## 7. How to Set Up This Project (Step by Step)

### Prerequisites
- Docker Desktop installed
- Git installed
- GitHub account
- (Optional) Azure free account

### Step 1: GitHub pe push karo
```bash
git init
git add .
git commit -m "initial commit: EmployeeApp"
# GitHub pe new repo banao
git remote add origin https://github.com/YOUR_USERNAME/EmployeeApp.git
git push -u origin main
```

### Step 2: Pipeline automatically chalegi
GitHub → Actions tab → `EmployeeApp CI/CD` pipeline → dekhte rahо

CI (build + test) automatic chalegi.

### Step 3: Docker job ke liye (main pe push hone par automatic)
GHCR pe image automatically jaayegi — koi setup nahi!
GitHub → Packages tab mein `employeeapp` image dikhegi.

### Step 4: Azure deploy ke liye (optional)
```bash
# Azure CLI se
az group create --name EmployeeApp-RG --location centralindia

az appservice plan create \
  --name EmployeeApp-Plan \
  --resource-group EmployeeApp-RG \
  --sku F1 --is-linux

az webapp create \
  --resource-group EmployeeApp-RG \
  --plan EmployeeApp-Plan \
  --name employeeapp-demo \
  --deployment-container-image-name ghcr.io/YOUR_USERNAME/employeeapp:latest
```

GitHub Secret add karo:
→ Repo Settings → Secrets → `AZURE_PUBLISH_PROFILE`
→ Azure Portal → App Service → Get publish profile → file content paste karo

Ab push karo → automatic deploy!

### Step 5: Deploy skip karna ho to
ci-cd.yml mein deploy job ko comment kar do:
```yaml
# deploy:
#   name: Deploy to Azure
#   ...
```
Sirf build + test + docker chalega — deploy nahi.

---

## Interview Answer — Ye Project Explain Karo

> "Maine ek ASP.NET Core 8 project banaya jisme Employee aur Department ka CRUD hai — EF Core InMemory database ke saath. Swagger integrate kiya taaki API directly browser se test ho sake. Docker mein multi-stage Dockerfile likhi — SDK stage mein code compile kiya, runtime stage mein sirf output copy kiya — final image ~200MB ki bani. GitHub Actions mein teen jobs hain: build-and-test jo har push pe chalta hai aur dotnet test se 17 integration tests run karta hai, docker-build-push jo sirf main branch pe chalta hai aur GHCR pe image push karta hai, aur deploy job jo Azure App Service pe same tested image deploy karta hai. Agar koi test fail ho to Docker build aur deploy automatically rok jaate hain — broken code kabhi production nahi jaata. GITHUB_TOKEN use kiya GHCR login ke liye — koi extra secret set nahi karna pada."
