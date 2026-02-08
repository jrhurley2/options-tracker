# Contributing to Options Tracker

Thank you for your interest in contributing! Here's how you can help.

## Development Setup

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server (LocalDB, Express, or Docker)
- Git

### Getting Started

1. **Fork the repository** on GitHub

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/options-tracker.git
   cd options-tracker
   ```

3. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

4. **Set up the backend**
   ```bash
   cd Backend
   dotnet restore
   dotnet run
   ```

5. **Set up the frontend** (in a new terminal)
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```

## Coding Standards

### Backend (C#)
- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Write unit tests for business logic

### Frontend (TypeScript/React)
- Use TypeScript for type safety
- Follow React best practices and hooks patterns
- Use functional components
- Keep components small and reusable
- Use Tailwind CSS for styling (no custom CSS unless necessary)

## Branch Naming

- `feature/` - New features (e.g., `feature/add-greeks-calculation`)
- `fix/` - Bug fixes (e.g., `fix/csv-import-date-parsing`)
- `refactor/` - Code refactoring (e.g., `refactor/simplify-position-service`)
- `docs/` - Documentation updates (e.g., `docs/update-api-examples`)

## Commit Messages

Follow conventional commits:

```
type(scope): brief description

Longer description if needed

- Bullet points for details
- Another detail
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style/formatting
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

Examples:
```bash
git commit -m "feat(options): add option Greeks calculation"
git commit -m "fix(csv): handle Schwab date format variations"
git commit -m "docs(readme): add Docker deployment instructions"
```

## Pull Request Process

1. **Update your branch** with the latest from main
   ```bash
   git checkout main
   git pull upstream main
   git checkout feature/your-feature
   git rebase main
   ```

2. **Test your changes**
   - Backend: Run `dotnet test` (once tests exist)
   - Frontend: Run `npm run build` to ensure it compiles
   - Test manually in the browser

3. **Push to your fork**
   ```bash
   git push origin feature/your-feature
   ```

4. **Create a Pull Request** on GitHub
   - Use a clear, descriptive title
   - Explain what changes you made and why
   - Reference any related issues (#123)
   - Add screenshots for UI changes

5. **Respond to review feedback**
   - Make requested changes
   - Push new commits to the same branch
   - Reply to comments when done

## Feature Requests

Before working on a major feature:

1. **Open an issue** to discuss it first
2. Wait for maintainer feedback
3. Get approval before starting work

This prevents duplicate efforts and ensures alignment with project goals.

## Bug Reports

When reporting bugs, include:

- **Environment**: OS, .NET version, Node version, browser
- **Steps to reproduce**: Detailed steps
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Screenshots**: If applicable
- **Logs**: Backend logs or browser console errors

## Code Review Checklist

Before submitting a PR, verify:

- [ ] Code builds without errors
- [ ] Changes are tested manually
- [ ] No console errors or warnings
- [ ] Code follows project style
- [ ] Commit messages are clear
- [ ] PR description explains the changes
- [ ] No unnecessary files included (node_modules, bin, obj)

## Questions?

- Open a GitHub Discussion for general questions
- Open an Issue for bug reports or feature requests
- Tag maintainers with @mention if you need clarification

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🎉
