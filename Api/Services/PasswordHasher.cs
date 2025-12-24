namespace Api.Services
{
    public sealed class PasswordHasher
    {
        public string Hash(string password) => Common.Hash.Hash.ComputeSHA256(password);

        public bool Verify(string password, string hash) => Common.Hash.Hash.CompareSHA256(password, hash);
    }

}
