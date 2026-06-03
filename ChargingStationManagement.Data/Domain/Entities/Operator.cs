namespace ChargingStationManagement.Domain.Entities;

public class Operator
{
    public Guid Id { get; private set; }
    public string OperatorId { get; private set; } = null!;
    public string OperatorName { get; private set; } = null!;

    private Operator() { }

    public Operator(string operatorId, string operatorName)
    {
        Id = Guid.NewGuid();
        OperatorId = operatorId;
        OperatorName = operatorName;
    }

    public void UpdateName(string name) => OperatorName = name;

    internal void UpdateApiCredentials(string v1, string v2)
    {
        //throw new NotImplementedException();
    }
}