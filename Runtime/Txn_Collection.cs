using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AlturaNFT  
{ using Internal;
    
    /// <summary>
    /// Get Transactions of an NFT
    /// </summary>
    [AddComponentMenu(AlturaConstants.BaseComponentMenu+AlturaConstants.FeatureName_Txn_Collection)]
    [ExecuteAlways]
    [HelpURL(AlturaConstants.Txn_Collection)]
    public class Txn_Collection : MonoBehaviour
    {
        /// <summary>
        /// Currently Supported chains for this endpoint.
        /// </summary>
        public enum Chains
        {
            ethereum,
            binance,
            bsctest,
        }
        

        #region Parameter Defines

        [SerializeField] private Chains chain;
        
            [SerializeField]
            [Tooltip("Input Collection_id / Contract Address of the NFTs")]
            private string _collection_address = "Input Collection/Contract Address of the NFTs";
        


            private string RequestUriInit = "https://api.alturanft.com/api/v2/collection/";
            private string WEB_URL;
            private string _apiKey;
            private bool destroyAtEnd = false;


            private UnityAction<string> OnErrorAction;
            private UnityAction<Collection_model> OnCompleteAction;
            
            [Space(20)]
            //[Header("Called After Successful API call")]
            public UnityEvent afterSuccess;
            //[Header("Called After Error API call")]
            public UnityEvent afterError;

            [Header("Run Component when this Game Object is Set Active")]
            [SerializeField] private bool onEnable = false;
            public bool debugErrorLog = true;
            public bool debugLogRawApiResponse = false;
            
            [Header("Gets filled with data and can be referenced:")]
            public Collection_model collectionModel;

        #endregion


        private void Awake()
        {
            AlturaUser.Initialise();
            _apiKey = AlturaUser.GetUserApiKey();
            
        }

        private void OnEnable()
        {
            if (onEnable & Application.isPlaying)
            {
                AlturaUser.SetFromOnEnable();
                Run();
            }
        }

        #region SetParams and Chain Functions

        /// <summary>
        /// Initialize creates a gameobject and assings this script as a component. This must be called if you are not refrencing the script any other way and it doesn't already exists in the scene.
        /// </summary>
        /// <param name="destroyAtEnd"> Optional bool parameter can set to false to avoid Spawned GameObject being destroyed after the Api process is complete. </param>
        public static Txn_Collection Initialize(bool destroyAtEnd = true)
            {
                var _this = new GameObject(AlturaConstants.FeatureName_Txn_Collection).AddComponent<Txn_Collection>();
                _this.destroyAtEnd = destroyAtEnd;
                _this.onEnable = false;
                _this.debugErrorLog = false;
                return _this;
            }

        /// <summary>
        /// Set Parameters to retrieve NFT From.  ??????????????? .
        /// </summary>
        /// <param name="collection_address"> as string - EVM</param>
        /// <param name="type"> as Type{ all, mint, burn, transfer_from, transfer_to, list, buy, sell, make_bid , get_bid}.</param>
        public Txn_Collection SetParameters(string collection_address = null)
        {
            if(collection_address!=null)
                this._collection_address = collection_address;
            return this;
        }

        /// <summary>
        /// Blockchain from which to query NFTs.
        /// </summary>
        /// <param name="chain"> Choose from available 'Chains' enum</param>
        public Txn_Collection SetChain(Chains chain)
        {
            this.chain = chain;
            return this;
        }

        /// <summary>
        /// Action on successful API Fetch. (*^???^)???(?????????*)
        /// </summary>
        /// <param name="Collection_model"> Use: .OnComplete(Txns=> txns = Txns) , where txns is of type Collection_model;</param>
        /// <returns> NFTs_OwnedByAnAccount_model.Root </returns>
        public Txn_Collection OnComplete(UnityAction<Collection_model> action)
        {
            this.OnCompleteAction = action;
            return this;
        }
        
        /// <summary>
        /// </summary>
        /// <param name="UnityAction action"> string.</param>
        /// <returns> Information on Error as string text.</returns>
        public Txn_Collection OnError(UnityAction<string> action)
        {
            this.OnErrorAction = action;
            return this;
        }
            
        #endregion

        
        #region Run - API
            /// <summary>
            /// Runs the Api call and fills the corresponding model in the component on success.
            /// </summary>
            public Collection_model Run()
            {
                WEB_URL = BuildUrl();
                StopAllCoroutines();
                StartCoroutine(CallAPIProcess());
                return collectionModel;
            }

            string BuildUrl()
            {
                if (chain == Chains.binance)
                {
                    WEB_URL = RequestUriInit + _collection_address;
                }
                else
                {
                    WEB_URL = RequestUriInit + _collection_address;
                }
                
                if(debugErrorLog)
                    Debug.Log("Querying Transactions of Collection: " + _collection_address + " on " + chain);
                
                return WEB_URL;
            }
            
            IEnumerator CallAPIProcess()
            {
                //Make request
                UnityWebRequest request = UnityWebRequest.Get(WEB_URL);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("source", AlturaUser.GetSource());
                request.SetRequestHeader("Authorization", _apiKey);
                
                {
                    yield return request.SendWebRequest();
                    string jsonResult = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                    if(debugLogRawApiResponse)
                        Debug.Log(jsonResult);

                    if (request.error != null)
                    {
                        if(OnErrorAction!=null)
                            OnErrorAction($"Null data. Response code: {request.responseCode}. Result {jsonResult}");
                        if(debugErrorLog)
                            Debug.Log($" Response code: {request.responseCode}. Result {jsonResult}");
                        if(afterError!=null)
                            afterError.Invoke();
                        collectionModel = null;
                        //yield break;
                    }
                    else
                    {
                        collectionModel = JsonConvert.DeserializeObject<Collection_model>(
                            jsonResult,
                            new JsonSerializerSettings
                            {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                            });
                        
                        if(OnCompleteAction!=null)
                            OnCompleteAction.Invoke(collectionModel);
                        
                        if(afterSuccess!=null)
                            afterSuccess.Invoke();
                        
                        if(debugErrorLog)
                            Debug.Log($"Response: Success , view Collections model" );
                    }
                }
                request.Dispose();
                if(destroyAtEnd)
                    Destroy (this.gameObject);
            }
            
        #endregion
    }

}
