# ═══════════════════════════════════════════════════════════════════════════════
# DOCKERFILE — Multi-stage build
#
# Stage 1 (build):   SDK image use karo — code compile karo
# Stage 2 (runtime): Sirf runtime image — chhoti final image
#
# Why multi-stage?
#   SDK image  = ~800MB (build tools, compilers sab hai)
#   Runtime image = ~200MB (sirf app chalane ka)
#   Final Docker image = ~200MB — server pe kam space, fast pull
# ═══════════════════════════════════════════════════════════════════════════════

# ── Stage 1: BUILD ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# TRICK: pehle sirf .csproj copy karo aur restore karo
# Kyun? Docker har instruction ka cache banata hai (layer cache)
# Agar sirf code change hua aur .csproj nahi — restore skip hoga
# Result: faster builds on subsequent runs
COPY EmployeeApp/EmployeeApp.csproj EmployeeApp/
RUN dotnet restore EmployeeApp/EmployeeApp.csproj \
    --disable-parallel

# Ab baaki code copy karo aur publish karo
COPY EmployeeApp/ EmployeeApp/
# --no-restore hata diya — Windows NuGet cache path Linux mein nahi milta
# dotnet publish khud restore karega fresh Linux paths ke saath
RUN dotnet publish EmployeeApp/EmployeeApp.csproj \
    -c Release \
    -o /app/publish

# ── Stage 2: RUNTIME ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security best practice: root user se mat chalao
# Agar koi container exploit kare to host pe access na ho
# RUN adduser --disabled-password --gecos "" appuser \
#     && chown -R appuser /app
# USER appuser

# Sirf compiled output copy karo — source code nahi jaata image mein
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://localhost:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EmployeeApp.dll"]