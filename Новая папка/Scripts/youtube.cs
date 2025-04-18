using UnityEngine;
using UnityEngine.UI;
using System; 
using UnityEngine.Networking; 

public class youtube : MonoBehaviour
{

    [Header("��������� ������")]
    [Tooltip("������ URL (���������� � http:// ��� https://)")]
    [SerializeField] private string targetURL = "https://example.com";

    [Header("��������� ������")]
    [SerializeField] private Button clickButton;
    [SerializeField] private bool checkInternetConnection = true;
    [SerializeField] private bool validateURL = true;

    [Header("���������� �������� �����")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color loadingColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    private Color originalColor;

    private void Start()
    {
        
        if (clickButton == null)
        {
            clickButton = GetComponent<Button>();
            if (clickButton == null)
            {
                Debug.LogError("�� ������� ������ �� ���� �������!");
                return;
            }
        }

        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }

        
        clickButton.onClick.AddListener(OpenURL);
    }

    public async void OpenURL()
    {
        
        SetButtonColor(loadingColor);

        
        if (checkInternetConnection && !await CheckInternetConnection())
        {
            Debug.LogError("��� ��������-����������!");
            SetButtonColor(errorColor);
            return;
        }

      
        if (validateURL && !IsValidURL(targetURL))
        {
            Debug.LogError("������������ URL: " + targetURL);
            SetButtonColor(errorColor);
            return;
        }

        
        try
        {
            Application.OpenURL(targetURL);
            Debug.Log("������� ������ URL: " + targetURL);
            SetButtonColor(successColor);

            Invoke(nameof(ResetButtonColor), 1f);
        }
        catch (Exception e)
        {
            Debug.LogError("������ ��� �������� URL: " + e.Message);
            SetButtonColor(errorColor);
        }
    }

    private bool IsValidURL(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async System.Threading.Tasks.Task<bool> CheckInternetConnection()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://google.com"))
        {
            var asyncOp = webRequest.SendWebRequest();

            // ���� �� 3 ������
            float timeout = 3f;
            float elapsed = 0f;

            while (!asyncOp.isDone && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }

            return !webRequest.isNetworkError && !webRequest.isHttpError;
        }
    }

    private void SetButtonColor(Color color)
    {
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    private void ResetButtonColor()
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

   
    public void SetTargetURL(string newURL)
    {
        targetURL = newURL;
    }
}