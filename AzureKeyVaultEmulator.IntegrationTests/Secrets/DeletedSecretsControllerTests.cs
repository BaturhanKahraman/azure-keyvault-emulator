﻿using Azure.Security.KeyVault.Secrets;
using AzureKeyVaultEmulator.IntegrationTests.SetupHelper;
using AzureKeyVaultEmulator.IntegrationTests.SetupHelper.Fixtures;

namespace AzureKeyVaultEmulator.IntegrationTests.Secrets
{
    public class DeletedSecretsControllerTests(SecretsTestingFixture fixture) : IClassFixture<SecretsTestingFixture>
    {
        [Fact]
        public async Task GetDeletedSecretReturnsPreviousDeletedSecretTest()
        {
            var client = await fixture.GetClientAsync();

            var secretName = "getDeletedSecretName";
            var secretValue = "getSelectedSecretPassword";

            var secret = await fixture.CreateSecretAsync(secretName, secretValue);

            var deletedAction = await client.StartDeleteSecretAsync(secret.Name);

            Assert.Equal(secret.Name, deletedAction.Value.Name);

            await Assert.RequestFailsAsync(() => client.GetSecretAsync(secret.Name));
           
            var fromDeletedSource = await client.GetDeletedSecretAsync(secret.Name);

            Assert.Equal(secret.Name, fromDeletedSource.Value.Name);
        }

        [Fact]
        public async Task GetDeletedSecretsPagesForCorrectCountTest()
        {
            var client = await fixture.GetClientAsync();

            var multiSecretName = "multiDelete";

            var executionCount = await RequestSetup
                .CreateMultiple(26, 51, i => client.SetSecretAsync(multiSecretName, $"{i}value", fixture.CancellationToken));

            var deletedOperation = await client.StartDeleteSecretAsync(multiSecretName);

            Assert.Equal(multiSecretName, deletedOperation.Value.Name);

            var deletedSecrets = new List<DeletedSecret>();

            var deletePager = client.GetDeletedSecretsAsync(fixture.CancellationToken);

            await foreach (var deletedSecret in deletePager)
                if (deletedSecret.Name.Contains(multiSecretName, StringComparison.CurrentCultureIgnoreCase))
                    deletedSecrets.Add(deletedSecret);

            Assert.Single(deletedSecrets);
        }

        [Fact]
        public async Task PurgeDeletedSecretRemovesFromDeletedCacheTest()
        {
            var client = await fixture.GetClientAsync();

            var secretName = "purgingSecret";
            var secretValue = "purgedValue";

            var secret = await fixture.CreateSecretAsync(secretName, secretValue);

            Assert.Equal(secretName, secret.Name);

            var deletedOperation = await client.StartDeleteSecretAsync(secretName);

            Assert.Equal(secretName, deletedOperation?.Value.Name);

            var deletedSecretFromCache = await client.GetDeletedSecretAsync(secretName);

            Assert.Equal(secretName, deletedSecretFromCache?.Value.Name);

            await client.PurgeDeletedSecretAsync(secretName);

            await Assert.RequestFailsAsync(() => client.GetDeletedSecretAsync(secretName));

            await Assert.RequestFailsAsync(() => client.GetSecretAsync(secretName));
        }

        [Fact]
        public async Task RecoverDeletedSecretRestoresToPrimaryCacheTest()
        {
            var client = await fixture.GetClientAsync();

            var secretName = "recoveredDeletedSecret";
            var secretValue = "recoveredPassword";

            var secret = await fixture.CreateSecretAsync(secretName, secretValue);

            Assert.Equal(secretName, secret.Name);

            var deletedOperation = await client.StartDeleteSecretAsync(secretName);

            Assert.Equal(secretName, deletedOperation.Value.Name);

            var recovered = await client.StartRecoverDeletedSecretAsync(secretName);

            Assert.Equal(secretName, recovered.Value.Name);

            var afterRecovery = await client.GetSecretAsync(secretName);

            Assert.Equal(secretName, afterRecovery.Value.Name);

            await Assert.RequestFailsAsync(() => client.GetDeletedSecretAsync(secretName));

        }
    }
}
