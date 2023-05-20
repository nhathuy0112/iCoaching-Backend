namespace Infrastructure.Utils;

public static class DateConverter
{
    public static DateTime AddFromStartDate(string time, string unit, DateTime startDate)
    {
        foreach (var methodInfo in startDate.GetType().GetMethods())
        {
            if (!methodInfo.Name.ToLower().Contains(("Add" + unit).ToLower())) continue;
            
            var paramArray = new object[] { int.Parse(time) };
            return (DateTime) startDate
                .GetType()
                .GetMethod(methodInfo.Name)
                .Invoke(startDate, paramArray);
        }

        return DateTime.Now.AddMinutes(int.Parse(time));
    }
}