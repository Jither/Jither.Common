using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jither.CommandLine
{
    public enum IssueType
    {
        NonUniqueShortName,
        NonUniqueLongName,
        NonUniquePosition,
        RequiredSwitch,
        SwitchWithDefault,
        RequiredAfterNotRequired,
        RequiredWithDefault,
        InvalidExample,
        ListOfBooleans
    }

    public class Issue
    {
        public string VerbName { get; }
        public IssueType Type { get; }
        public string Message { get; }

        public Issue(string verbName, IssueType issue, string message)
        {
            VerbName = verbName;
            Type = issue;
            Message = message;
        }

        public override string ToString()
        {
            if (VerbName != null)
            {
                return $"{VerbName}: {Message}";
            }
            return Message;
        }
    }

    public class IssueCollection : IEnumerable<Issue>
    {
        private List<Issue> issues = new List<Issue>();

        public void Add(Verb verb, IssueType type, string message)
        {
            issues.Add(new Issue(verb.Name ?? "default", type, message));
        }

        public IEnumerator<Issue> GetEnumerator()
        {
            return issues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, issues.Select(issue => $"* {issue}"));
        }
    }

    public class VerbSanityChecker
    {
        public VerbSanityChecker()
        {
        }

        public IssueCollection FindIssues(Verb options, IEnumerable<Verb> verbs = null)
        {
            var result = new IssueCollection();

            if (options != null)
            {
                FindVerbIssues(result, options);
            }
            if (verbs != null)
            {
                foreach (var verb in verbs)
                {
                    FindVerbIssues(result, verb);
                }
            }
            return result;
        }

        private void FindVerbIssues(IssueCollection issues, Verb verb)
        {
            CheckUniqueNames(issues, verb);
            CheckUniquePositionals(issues, verb);
            CheckRequiredPositionals(issues, verb);
            CheckRequiredWithDefault(issues, verb);
            CheckSwitches(issues, verb);
            CheckExamples(issues, verb);
            CheckLists(issues, verb);
        }

        private void CheckExamples(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();
            var examples = verb.GetExamples();
            var formatter = new ArgumentsFormatter(verb);
            foreach (var example in examples)
            {
                try
                {
                    verb.Parse(example.GetArguments(formatter), false);
                }
                catch (Exception ex)
                {
                    issues.Add(verb, IssueType.InvalidExample, $"Example '{example.Help}' failed validation: {ex.Message}");
                }
            }
        }

        private void CheckUniqueNames(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            var nonUniqueShortNames = defs.Options
                .Where(o => o.ShortNameCharacter != null)
                .Select(o => o.ShortNameCharacter.Value)
                .GroupBy(o => o)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            var nonUniqueLongNames = defs.Options
                .Where(o => o.Name != null)
                .Select(o => o.Name)
                .GroupBy(o => o)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var shortName in nonUniqueShortNames)
            {
                issues.Add(verb, IssueType.NonUniqueShortName, $"The short name -{shortName} is used more than once.");
            }
            foreach (var longName in nonUniqueLongNames)
            {
                issues.Add(verb, IssueType.NonUniqueLongName, $"The long name --{longName} is used more than once.");
            }
        }

        private void CheckUniquePositionals(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            var nonUniquePositionals = defs.Positionals
                .GroupBy(o => o.Position)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var position in nonUniquePositionals)
            {
                issues.Add(verb, IssueType.NonUniquePosition, $"The position {position} is used more than once.");
            }
        }

        private void CheckRequiredWithDefault(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            foreach (var option in defs.Options)
            {
                if (option.Required && option.Default != null)
                {
                    issues.Add(verb, IssueType.RequiredWithDefault, $"Option {option.DisplayName} is required, but also has a default.");
                }
            }
        }

        private void CheckRequiredPositionals(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            bool previousWasRequired = true;

            foreach (var positional in defs.Positionals)
            {
                if (positional.Required && !previousWasRequired)
                {
                    issues.Add(verb, IssueType.RequiredAfterNotRequired, $"Positional {positional.DisplayName} at {positional.Position} is required, but the previous positional wasn't.");
                }
                previousWasRequired = positional.Required;
            }
        }

        private void CheckSwitches(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            foreach (var option in defs.Options.Where(o => o.IsSwitch))
            {
                if (option.Required)
                {
                    issues.Add(verb, IssueType.RequiredSwitch, $"Switch {option.DisplayName} is required - that would make it always true.");
                }
                if (option.Default != null)
                {
                    issues.Add(verb, IssueType.SwitchWithDefault, $"Switch {option.DisplayName} has a default - switches should always be false by default.");
                }
            }
        }

        private void CheckLists(IssueCollection issues, Verb verb)
        {
            var defs = verb.GetArgumentDefinitions();

            foreach (var option in defs.Options.Where(o => o.IsList))
            {
                if (option.PropertyType == typeof(bool))
                {
                    issues.Add(verb, IssueType.ListOfBooleans, $"{option.DisplayName} is a list of booleans/switches - that has little use.");
                }
            }
        }

    }
}
