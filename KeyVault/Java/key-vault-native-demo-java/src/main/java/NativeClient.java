import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URI;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

import javax.naming.ServiceUnavailableException;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.utils.URIBuilder;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;

import org.json.JSONObject;

import com.microsoft.aad.adal4j.AuthenticationContext;
import com.microsoft.aad.adal4j.AuthenticationResult;
import com.microsoft.aad.adal4j.ClientCredential;

public class NativeClient {
    private final static String RESOURCE = "https://vault.azure.cn";

    public static void main(String args[]) throws Exception {
    	// TODO: update to match your environment
    	String tenantId = "";
        String clientId = "";
        String clientKey = "";
        String restApiUrl = "";
        
        AuthenticationResult authResult = getAccessTokenFromClientCredentials(tenantId, clientId, clientKey);
        String accessToken = authResult.getAccessToken();
        System.out.println("Access Token - " + accessToken);
        
        String connectionString = GetSecretFromKeyVault(restApiUrl, accessToken);
        System.out.println("Connection String retrieved from Key Vault - " + connectionString);
    }
    
    private static AuthenticationResult getAccessTokenFromClientCredentials(
            String tenantId, String clientId, String clientKey) throws Exception {
    	String authority = "https://login.chinacloudapi.cn/" + tenantId;
        AuthenticationContext context = null;
        AuthenticationResult result = null;
        ExecutorService service = null;
        ClientCredential clientCredential = null;
        try {
            service = Executors.newFixedThreadPool(1);
            context = new AuthenticationContext(authority, false, service);
            clientCredential = new ClientCredential(clientId, clientKey);
            Future<AuthenticationResult> future = context.acquireToken(RESOURCE, clientCredential, null);
            result = future.get();
        }
        catch (Exception ex) {
        	System.out.println("Something wrong - " + ex.getMessage());
        }
        finally {
            service.shutdown();
        }

        if (result == null) {
            throw new ServiceUnavailableException("authentication result was null");
        }
        return result;
    }
    
    private static String GetSecretFromKeyVault(String keyVaultUrl, String accessToken)
    {    	
		HttpClient httpclient = HttpClients.createDefault();

		try {
			URIBuilder builder = new URIBuilder(keyVaultUrl);
			builder.setParameter("api-version", "2016-10-01");
			URI uri = builder.build();
			
			HttpGet request = new HttpGet(uri);
			request.setHeader("Content-Type", "application/json");
			request.setHeader("Authorization", "Bearer " + accessToken);

			HttpResponse response = httpclient.execute(request);
			HttpEntity entity = response.getEntity();

			if (entity != null) {
				String secretInJson = EntityUtils.toString(entity);
				JSONObject secretObj = new JSONObject(secretInJson);
				return secretObj.getString("value");
			}

			return "";
		} catch (Exception e) {
			System.out.println(e.getMessage());
			return "";
		}
    }
}
