module Domain.Contracts.GitHubRequest

type GitHubRequest = 
    {
        Title: string
        Body: string
        Labels: string array }