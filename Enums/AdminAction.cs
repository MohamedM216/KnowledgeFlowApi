namespace KnowledgeFlowApi.Enums
{
    public enum AdminAction
    {
        BanUser,    // warn user, ban user or permenant ban (decided automaticly)
        DeleteFile, // delete file & BanUser
        IncorrectReport
    }
}