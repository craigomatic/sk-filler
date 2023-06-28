public class FillState
{
    public readonly static FillState Incomplete = new FillState();

    public bool IsComplete { get; set; }

    public IEnumerable<FieldViewModel> Fields { get; set; } = Enumerable.Empty<FieldViewModel>();

    public float Progress
    {
        get
        {
            return this.Fields.Count(f => !string.IsNullOrEmpty(f.Value)) / this.Fields.Count();
        }
    }
}