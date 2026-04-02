namespace ATM.Backend.Data;

public enum MutationStatus
{
    Success,
    NotFound,
    Rejected
}

public sealed record AccountMutationResult(MutationStatus Status, string Message, AccountRecord? Account);

public sealed record TransferResult(MutationStatus Status, string Message, AccountRecord? SourceAccount);
