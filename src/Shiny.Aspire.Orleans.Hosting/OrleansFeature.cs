namespace Shiny.Aspire.Orleans.Hosting;

[Flags]
public enum OrleansFeature
{
    Clustering = 1,
    Persistence = 2,
    Reminders = 4,
    All = Clustering | Persistence | Reminders
}
