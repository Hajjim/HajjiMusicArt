using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;



namespace uOSC
{
    [RequireComponent(typeof(uOscClient))]
    public class CreateSphere : MonoBehaviour
    {


        //------------------------------------
        // Variables :
        //------------------------------------

        public int inPort = 8000;
        public GameObject SphereMusic;
        GameObject SphereMusicClone;
        List<GameObject> listSphere;
        Collider lastcollider;
        bool onerequest = false;

        public int star;
        public int numOfStars;
        public Image[] stars;
        public Sprite fullStar;
        public Sprite emptyStar;
        public GameObject lecanvas;

        public int presetnumber = 1;
        private int valeur;
        private Vector3 decalage;
        private int m = 0;
        private int n;

        private float maxRayDistance = 25;
        private Vector3 origin;
        private Vector3 direction;
        private SphereCollider sphere;
        private Vector3 start;
        private Vector3 end;
        private Vector3 VecHypo;
        private float Hypo;
        private float Adj;
        private float dist;
        private float oppose;
        private float t1;
        private float t0;
        private float ma; //mini adj
        private float r; //mini opposé
        private float R; //mini hypoth
        private float w = 0; //poids

        Stack<Color> mycolors = new Stack<Color>();

        //------------------------------------
        // Méthodes :
        //------------------------------------

        void Start()
        {
            mycolors.Push(Color.white);
            mycolors.Push(Color.black);
            mycolors.Push(Color.gray);
            mycolors.Push(Color.cyan);
            mycolors.Push(Color.magenta);
            mycolors.Push(Color.yellow);
            mycolors.Push(Color.blue);
            mycolors.Push(Color.green);
            mycolors.Push(Color.red);
            listSphere = new List<GameObject>();

            var client = GetComponent<uOscClient>();
            var server = GetComponent<uOscServer>();
            server.onDataReceived.AddListener(OnDataReceived);

            client.Send("/osc/respond_to", inPort); //Prévenir Paca du port où il doit répondre Vérifier avec outport
            Debug.Log("Connected to Pacarana ?");
            client.Send("/osc/notify/vcs/hajji", 1); //Prévenir qu'on souhaite recevoir les notifications (1 = ON) en cas de changement (vcs change, par exemple valeur widget /vcs,b)
            Debug.Log("Activation des notifications");
            client.Send("/osc/notify/presets/hajji", 1); //Demande du nombre de preset (notification active en cas de changement grâce à l'indice 1)
            Debug.Log("Demande du nombre de preset...");

            //mon preset silence
            n = 1;
            StartCoroutine(ChangeMusic());

            //client.Send("/osc/widget", 1); //pour le nom des variables

        }



        void FixedUpdate()
        {
            //MixedRealityCameraParent
            origin = GameObject.Find("MixedRealityCamera").transform.position;//la pos sur lequel raycast se trouve
            direction = GameObject.Find("MixedRealityCamera").transform.forward; //la direction du vecteur 
            //Raycast:----------------------------------------------------------------------------------------
            Ray ray = new Ray(origin, direction); //raycast
                                                  // RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance); //pour toucher à longue portée toutes mes spheres
            Debug.DrawLine(origin, GameObject.Find("MixedRealityCamera").transform.position + direction * maxRayDistance, Color.red); //dessin mon raycast sur la scène:

            //foreach (RaycastHit hit in hits) // si je veux calculer pour toutes les spheres touchées
            RaycastHit hit; //pour le solo
            if (Physics.Raycast(origin, direction, out hit))
            {
                //Calcul du poids:-------------------------------------------------------------------------------------
                sphere = (SphereCollider)hit.collider;
                //Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.cyan);  //j'affiche une verticale sur intersection touchée       
                VecHypo = sphere.transform.position - ray.origin; //position sphere (centre) - point dd'origine camera Raycast = Hypo vecteur
                Hypo = Vector3.Dot(VecHypo, VecHypo); //hypo produit vectoriel
                Adj = Vector3.Dot(VecHypo, direction); //Adjacente (hypo projeté) sur là où je regarde car hypo par rapport au centre
                oppose = Mathf.Sqrt((Hypo * Hypo) - (Adj * Adj)); //opposé
                dist = Adj - hit.distance; //distance entre là où je regarde (jusque centre projeté parallele) et là où je frappe avec hit
                t0 = Adj - dist;
                t1 = Adj + dist;
                start = ray.origin + direction * t0;
                end = ray.origin + direction * t1;
                ma = dist; //car joue le role de notre adj par rapport à moyenne de point t0 et t1
                R = (float) (sphere.radius*1.4 / 2); //car radius de sphere du collider = diamètre et 1.4 pour scale look
                r = Mathf.Sqrt((R * R) - (ma * ma));
                w = 1 - (r / R);
                //Debug.Log("le poid est de " + w);
                //Debug.DrawLine(start, start + Vector3.down, Color.blue);
                //Debug.DrawLine(end, end + Vector3.down, Color.green);

                //MessageOSC + Scale :------------------------------------------------------------------------------------
                n = hit.collider.GetComponent<InfoSphere>().presetno;
                if(lecanvas.GetComponent<CanvasGroup>().alpha != 1)
                {
                    lecanvas.GetComponent<CanvasGroup>().alpha = 1f;
                }
                    
                star = (int)(w * 5) + 1;
                //Debug.Log(star);
                if (w != 0 && n!=m)
                {               
                    StartCoroutine(ChangeMusic());
                    if (onerequest == true)
                    {
                       
                        m = n;
                        lastcollider.GetComponent<Renderer>().transform.localScale = lastcollider.GetComponent<Renderer>().transform.localScale / 1.4f;
                        onerequest = false;

                    }
                    if (onerequest == false)
                    {
                        m = n;
                        hit.collider.GetComponent<Renderer>().transform.localScale = hit.collider.GetComponent<Renderer>().transform.localScale * 1.4f;                                           
                        onerequest = true;
                        lastcollider = hit.collider;

                    }
                    
                }

            }
            else
            {
                w = 0;
                star = 0;
                lecanvas.GetComponent<CanvasGroup>().alpha = 0f;
                n = 1; // celui de mon ''Preset Silence''
                StartCoroutine(ChangeMusic());
                if (onerequest == true)
                {
                    lastcollider.GetComponent<Renderer>().transform.localScale = lastcollider.GetComponent<Renderer>().transform.localScale / 1.4f;
                    onerequest = false;
                }
            }

            //Stars Rating-------------------------------------

            if(star > numOfStars)
            {
                star = numOfStars;
            }

            for(int i = 0; i < stars.Length; i++){

                if (i < star)
                {
                    stars[i].sprite = fullStar;
                }
                else
                {
                    stars[i].sprite = emptyStar;
                }

                if (i < numOfStars){
                    stars[i].enabled = true;
                }
                else{
                    stars[i].enabled = false;
                }

            }


        }



        void OnApplicationQuit()
        {

            //Prévenir Paca que c'est terminé avec l'indice 0
            var client = GetComponent<uOscClient>();
            client.Send("/osc/respond_to", 0);
            Debug.Log("Disconnected from Pacarana");
        }



        //------------------------------------
        // Méthodes utilitaires :
        //------------------------------------



        IEnumerator ChangeMusic()  //Une co-routine renvoie toujours un type spécial : IEnumarator
        {
            if (n != m)
            {
                var client = GetComponent<uOscClient>();
                Debug.Log("Lancement du preset ciblé : " + n);
                client.Send("/preset", n);
                m = n;
                yield return null; //C’est au niveau de l’instruction yield que la pause s’effectue.
            }
        }




        void OnDataReceived(Message message)
        {
            // address---------------------------
            var msg = message.address + ": ";


            // timestamp------------------------
            msg += "(" + message.timestamp.ToLocalTime() + ") ";

            // values---------------------------
            foreach (var value in message.values)
            {
                msg += value.GetString() + " ";
                valeur = (int)(value); //revoir optimisation
            }

            Debug.Log(msg);
            //---------------------------------
            //pour savoir si co
            if (message.address == "/osc/response_from")
            {
                Debug.Log(msg + " => Connected to Pacarana");
            }
            //pour le nombre de mess
            if (message.address == "/osc/notify/presets/hajji")
            {
                presetnumber = valeur;
                Debug.Log(msg + " => Nombre : " + valeur);
                //Je créé le nombre de sphère requis : 
                for (int i = 1; i <= presetnumber; i++)
                {
                    string objectName = "SpherePreset_" + i; // Obtenir le nom avec / preset ... etc !!! => A optimiser
                                                             // float angleIteration = 360 / presetnumber;
                                                             // float currentRotation = angleIteration * i;
                    SphereMusicClone = Instantiate(SphereMusic) as GameObject;
                    SphereMusicClone.transform.position = new Vector3(this.transform.position.x + Random.Range(-0.5f, 0.5f), this.transform.position.y + Random.Range(-0.5f, 0.5f), this.transform.position.z + Random.Range(-0.2f, 0.5f));
                    // SphereMusicClone.transform.rotation = this.transform.rotation;
                    // SphereMusicClone.transform.Rotate(new Vector3(0, currentRotation, 0));
                    SphereMusicClone.transform.Translate(new Vector3(0, 0.3f, 3f));
                    SphereMusicClone.GetComponentInChildren<Renderer>().material.color = mycolors.Pop(); //9 preset max
                    SphereMusicClone.name = objectName;
                    InfoSphere scriptvaleur = SphereMusicClone.GetComponent<InfoSphere>();
                    scriptvaleur.presetno = i;
                    //scriptvaleur.presetname = ...

                }
            }
            /*
            if (message.address == "/osc/widget")
            {
                byte[] byteTexture = ObjectToByteArray(message.values);
                string b = ByteArrayToString(byteTexture);
                string json = JsonUtility.ToJson(message.values); //??
                Debug.Log(json);
                Debug.Log(b);

            }*/

            // recoit le nom du preset avec /osc/preset i (presetnumber) et change le nameobject + affiche son nom sur sphere ?
        }
        /*
        string ByteArrayToString(byte[] val)
        {
            string b = "";
            int len = val.Length;
            for (int i = 0; i < len; i++)
            {
                if (i != 0)
                {
                    b += ",";
                }
                b += val[i].ToString();
            }
            return b;
        }
        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }*/
    }
}
