namespace Travelogue.Repository.Bases.Responses;
public static class ResponseMessageHelper
{
    public static string FormatMessage(this string template, params object[] args)
    {
        return string.Format(template, args);
    }
}