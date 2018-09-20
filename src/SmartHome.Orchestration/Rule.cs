namespace SmartHome.Orchestration
{
    public class Rule
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public RuleCondition[] Conditions { get; set; }

        public RuleAction[] Actions { get; set; }
    }
}
