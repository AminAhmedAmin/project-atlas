namespace Atlas.Domain.Content;

/// <summary>Kinds of repeatable blocks shown on the home page.</summary>
public enum BlockKind
{
    /// <summary>A headline number. Title = value ("50+"), Text = label.</summary>
    Stat = 1,

    /// <summary>A client or partner. Title = name, ImageUrl = optional logo.</summary>
    ClientLogo = 2,

    /// <summary>A step of the delivery process. Title = step name, Text = description.</summary>
    ProcessStep = 3,

    /// <summary>A client quote. Text = quote, Title = person, Subtitle = role and company.</summary>
    Testimonial = 4,

    /// <summary>A frequently asked question. Title = question, Text = answer.</summary>
    Faq = 5,
}
