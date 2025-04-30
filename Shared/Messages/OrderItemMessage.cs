namespace Shared.Messages;

//bir event'in altında ekstra bilgiler tutulacak ise message'larda tutmalıyız.
public class OrderItemMessage
{
    public Guid ProductId { get; set; }
    public int Count { get; set; }

}
