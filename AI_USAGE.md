# AI Usage

## Tools used

**Claude (claude.ai)** — used it to scaffold the project structure, the fake corpus, and a first pass at the tests.

## What AI was good for

- Scaffolding the solution layout, the two `.csproj` files, and the class split.
- Generating the fake corpus with deliberately different content per tenant, so the isolation tests have something real to prove.

## What AI got wrong

**The tests were never actually run before I ran them.** The environment the code was generated in couldn't reach NuGet, so xUnit couldn't be installed there. The suite was verified through a substitute test harness and I was told plainly that real xUnit was unverified. Running `dotnet test` on my machine was the first genuine execution of it.

**Target framework mismatch.** The project targets `net8.0`, and I was told a .NET 10 SDK would be enough. It compiles, but the test host needs the matching runtime installed — `dotnet test` failed until I installed the .NET 8 runtime alongside 10.

**A corpus detail worth knowing:** the fake PII and the injected "ignore previous instructions" text have to sit in sentences the keyword search will actually reach. If they don't, the redaction and prompt-injection tests pass against nothing — they look fine and prove nothing. That was corrected during the build, and I confirmed it by running the CLI and seeing `[REDACTED_EMAIL]` and the injected sentence appear as real citations.

Verified with:

    dotnet run --project src/AiEvidenceReview -- "What are our breach notification obligations?"
    dotnet run --project src/AiEvidenceReview -- "Do we have audit rights?"

The first returns a citation containing `[REDACTED_EMAIL]`; the second returns
the injected sentence as a citation without acting on it.

## What I changed manually

- **Added a passport-number redaction rule** (`\b[A-Z]{2}\d{6}\b` → `[REDACTED_PASSPORT]`) to `Redactor.cs`, placed last in the rules array so it can't shadow the SSN or account-number patterns, with a matching `[InlineData]` case in `RedactionTests.cs`. 24 tests passing.
- **Installed the .NET 8 runtime** to get the test suite running after diagnosing the framework mismatch above.

## What I can explain

All of it. The parts I'd point at first:

- `ForTenant` in `DocumentStore.cs` — the only method that reads the corpus, which is what makes tenant isolation one thing to review rather than a check repeated in several places.
- The `Summarize` signature — it takes the matches, not the document store, so it has no path to a document search didn't find. That's why the answer can't outrun its citations.