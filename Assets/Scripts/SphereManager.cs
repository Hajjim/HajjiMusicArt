using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

namespace uOSC
{
    [RequireComponent(typeof(uOscClient))]
    public class SphereManager : MonoBehaviour
    {

        //------------------------------------
        // Variables :
        //------------------------------------

        public int inPort = 9000;
        public int silence = 4;
        public GameObject SphereMusic;
        private GameObject SphereMusicClone;
        bool onerequest = false;
        bool goGenetic = false;
        bool anothertime = false;
        public Vector2 IdValue;
        List<int> listID;
        private bool ok = false;
        private bool ok2 = false;
        private bool widgetDone = false;
        private int presetNoCorrect = 1;
        public InfoSphere scriptvaleur;
        private int justparent;

        public int star;
        public int numOfStars;
        public Image[] stars;
        public Sprite fullStar;
        public Sprite emptyStar;
        public GameObject lecanvas;
        public int starec;
        public int numOfStarecs;
        public Image[] starecs;
        public Sprite fullStarec;
        public Sprite emptyStarec;

        List<GameObject> allsphere;
        private int popini;
        private string childName;
        private int childpresetno;
        private int nokeep;

        private int nbwidg;
        private int nbwidgclean = 0;
        public int presetnumber;
        private object valeur;
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
        private GestureRecognizer recognizer;


        //------------------------------------
        // Json Class :
        //------------------------------------
        public class Rootobject
        {
            public string creationClass { get; set; }
            public Layout layout { get; set; }
            public string label { get; set; }
            public object lookOrNil { get; set; }
            public string displayType { get; set; }
            public int concreteEventID { get; set; }
            public int minimum { get; set; }
            public int maximum { get; set; }
            public int grid { get; set; }
            public string taper { get; set; }
            public bool showNumber { get; set; }
            public bool isGenerated { get; set; }
            public object reflectMarker { get; set; }
            public object tickMarksOrNil { get; set; }
        }

        public class Layout
        {
            public string creationClass { get; set; }
            public int leftOffset { get; set; }
            public int topOffset { get; set; }
            public int rightOffset { get; set; }
            public int bottomOffset { get; set; }
            public float leftFraction { get; set; }
            public float topFraction { get; set; }
            public float rightFraction { get; set; }
            public float bottomFraction { get; set; }
        }


        //------------------------------------
        // Méthodes principales :
        //------------------------------------
        //***************************************************************************************************************************//

        void Start()
        {
            mycolors.Push(Color.gray);
            mycolors.Push(Color.black);
            mycolors.Push(Color.white);
            mycolors.Push(Color.cyan);
            mycolors.Push(Color.magenta);
            mycolors.Push(Color.yellow);
            mycolors.Push(Color.blue);
            mycolors.Push(Color.green);
            mycolors.Push(Color.red);

            listID = new List<int>();
            allsphere = new List<GameObject>();

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
            //n = silence;µ
            //se positionne preset 1
            //n = 1;
            //StartCoroutine(ChangeMusic());

        }

        //***************************************************************************************************************************//
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
                valeur = value; //revoir optimisation
            }

            //pour savoir si co-------------------------
            if (message.address == "/osc/response_from")
            {
                Debug.Log(msg + " => Connected to Pacarana");
            }

            //pour savoir le nb de widget
            if (message.address == "/osc/notify/vcs/hajji")
            {
                nbwidg = (int)valeur;
                Debug.Log("nombre de widget : " + nbwidg);
                ok = true;//pour update la liste des preset
            }

            //pour le nombre de sph------------------------
            if (message.address == "/osc/notify/presets/hajji" && anothertime == false)
            {
                presetnumber = (int)valeur;
                Debug.Log(msg + " => Nombre : " + valeur);
                //Je créé le nombre de sphère requis : 
                //population initial composé de presetnumber chromosomes qui sont composé de nwidget gênes
                for (int i = 1; i <= presetnumber; i++)
                {
                    if (i != silence) //preset silence degagé
                    {
                        string objectName = "SpherePreset_" + i;
                        // float angleIteration = 360 / presetnumber;
                        // float currentRotation = angleIteration * i;
                        SphereMusicClone = Instantiate(SphereMusic) as GameObject;
                        float rx = (Random.Range(0, 2) < 1) ? Random.Range(-0.6f, -0.3f) : Random.Range(0.3f, 0.6f);
                        float ry = (Random.Range(0, 2) < 1) ? Random.Range(-0.6f, -0.3f) : Random.Range(0.3f, 0.6f);
                        SphereMusicClone.transform.position = new Vector3(this.transform.position.x + rx, this.transform.position.y + ry, this.transform.position.z + Random.Range(-0.3f, 1f));
                        // SphereMusicClone.transform.rotation = this.transform.rotation;
                        // SphereMusicClone.transform.Rotate(new Vector3(0, currentRotation, 0));
                        SphereMusicClone.transform.Translate(new Vector3(0, 0.3f, 3f));
                        SphereMusicClone.GetComponentInChildren<Renderer>().material.color = changeAlpha(mycolors.Pop(), 0.3f); //9 preset max
                        SphereMusicClone.name = objectName;
                        scriptvaleur = SphereMusicClone.GetComponent<InfoSphere>();
                        scriptvaleur.presetno = i;
                        allsphere.Add(SphereMusicClone);

                    }
                }
                popini = allsphere.Count;
                childpresetno = popini + 1; //+1 car silence pas compté dans popini
                anothertime = true;
            }

            //après bundle que j'ai send avec /osc/widget,i-----------------------------------
            if (message.address == "/osc/widget") //value[0] = index of widget and value[1] = JSON String
            {
                var jsonstring = (string)message.values[1];
                Rootobject json = JsonConvert.DeserializeObject<Rootobject>(jsonstring);
                if (json != null && json.label != "Trigger" && json.concreteEventID != 0)
                {
                    nbwidgclean = nbwidgclean + 1;
                    listID.Add(json.concreteEventID); //ajout dans ma liste d'eventID
                    for (int i = 0; i < allsphere.Count; i++)
                    {
                        IdValue.x = json.concreteEventID;
                        IdValue.y = json.maximum;
                        allsphere[i].GetComponent<InfoSphere>().widgetValue.Add(IdValue);
                        //Debug.Log("json " + IdValue.x + " nom : " + json.label);
                    }
                }
                ok2 = true;
            }

            //ENCODAGE GENETIC----------------------------------------------------------
            //After switch preset, get /vcs,b  (b = message.values[0])
            //Palier le probleme que le preset est pas précisé...Chiant + j'ai du initialisé JSON avant pour eviter soucis EventID manquant
            if (message.address == "/vcs" && widgetDone == false) //evite calcul inutile le widgetdone et bug aussi du dernier preset qui change constant
            {
                Debug.Log("Encodage (via blob) du preset n°" + presetNoCorrect);
                byte[] blobpreset = (byte[])message.values[0];

                if (blobpreset.Length > 8) //pour éviter message inutile envoyé constant
                {
                    for (int i = 0; i <= blobpreset.Length - 8; i += 8)
                    {
                        SwapBytes(blobpreset, i);
                        SwapBytes(blobpreset, i + 4);
                        int eventID = System.BitConverter.ToInt32(blobpreset, i);
                        float valueWidget = System.BitConverter.ToSingle(blobpreset, i + 4);
                        //trop de calcul car leur systeme mal foutu...
                        if (valueWidget != 0.12345f)
                        {
                            //Debug.Log("Preset" + presetNoCorrect + "  EventID : " + eventID + "  ValueWidget : " + valueWidget);
                            CorrectValue(eventID, valueWidget, presetNoCorrect);
                            //vérification si on a deja tout parcouru niveau widget (évite erreur sur dernier widget modifié constant par changement de preset)
                            if (presetNoCorrect == (popini + 1) && i == blobpreset.Length - 16) //donc on a parcouru tout les widget du dernier preset
                            {
                                widgetDone = true; //on arrete les modif, on a tout ce qui nous faut
                                var client = GetComponent<uOscClient>();
                                client.Send("/preset", silence);
                            }
                        }
                    }

                }
            }
        }

        //***************************************************************************************************************************//
        //RESOLUTION BLOB => methode pour littleEndian systeme (Windows mais pas besoin pour Mac)
        void SwapBytes(byte[] aBytes, int aIndex)
        {
            var b0 = aBytes[aIndex + 0];
            var b1 = aBytes[aIndex + 1];
            var b2 = aBytes[aIndex + 2];
            var b3 = aBytes[aIndex + 3];
            aBytes[aIndex + 0] = b3;
            aBytes[aIndex + 1] = b2;
            aBytes[aIndex + 2] = b1;
            aBytes[aIndex + 3] = b0;
        }

        //***************************************************************************************************************************//
        void FixedUpdate()
        {
            //Initialisation widget de chaque sphere------------------
            if (ok == true && nbwidg != 0)
            {
                JsonEachWidget(); //récup les eventID et initialise à max all widget
                ok = false;
            }
            if (ok2 == true)
            {
                //Déclenchement de Blob-----------------------------------------------
                //je traite ainsi tout mes blob de chaque preset dès le début mais la méthode reste indésiré
                StartCoroutine(WaitPleaseSendBlob(0.2f)); //car blob trop lent à arriver donc 0.2 sec pour prendre en compte tout les blob
                //Déclenchement de Blob Propre-----------------------------------------------
                //méthode propre mais non fonctionnel sur kyma (retard sur la création de blob)
                //var bundlePreset = new Bundle(Timestamp.Now);
                //for (int i = 1; i < 9; i++) 
                //{
                //    if (i != silence) //preset silence degagé
                //    {
                //        bundlePreset.Add(new Message("/preset", i));
                //    }
                //}
                //client.Send(bundlePreset);
                ok2 = false;
            }

            //Stars Rating Affichage-------------------------------------
            if (star > numOfStars)
                star = numOfStars;
            for (int i = 0; i < stars.Length; i++)
            {
                //en cas d'erreur
                if (i < star)
                    stars[i].sprite = fullStar;
                else
                    stars[i].sprite = emptyStar;

                if (i < numOfStars)
                    stars[i].enabled = true;
                else
                    stars[i].enabled = false;
            }//-------------------------------------------------------------
            //Stars Rating Affichage Recorded-------------------------------
            if (starec > numOfStarecs)
                starec = numOfStarecs;
            for (int i = 0; i < starecs.Length; i++)
            {
                if (i < starec)
                    starecs[i].sprite = fullStarec;
                else
                    starecs[i].sprite = emptyStarec;

                if (i < numOfStarecs)
                    starecs[i].enabled = true;
                else
                    starecs[i].enabled = false;
            }//------------------------------------------------------------

            //Partie Réalité augmenté calcule-------------------------------------------------------------------------
            origin = GameObject.Find("MixedRealityCamera").transform.position;//la pos sur lequel raycast se trouve
            direction = GameObject.Find("MixedRealityCamera").transform.forward; //la direction du vecteur 
            Ray ray = new Ray(origin, direction);
            Debug.DrawLine(origin, GameObject.Find("MixedRealityCamera").transform.position + direction * maxRayDistance, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit) && hit.collider.GetType() == typeof(SphereCollider))
            {
                sphere = (SphereCollider)hit.collider;
                //Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.cyan);  //j'affiche une verticale sur intersection touchée       
                VecHypo = sphere.transform.position - ray.origin; //position sphere (centre) - point dd'origine camera Raycast = Hypo vecteur
                Adj = Vector3.Dot(VecHypo, direction); //Adjacente (hypo projeté) sur là où je regarde car hypo par rapport au centre
                dist = Adj - hit.distance; //distance entre là où je regarde (jusque centre projeté parallele) et là où je frappe avec hit
                t0 = Adj - dist;
                //t1 = Adj + dist;
                start = ray.origin + direction * t0;
                //end = ray.origin + direction * t1;
                ma = dist; //car joue le role de notre adj par rapport à moyenne de point t0 et t1
                R = (float)(sphere.radius / 2);
                r = Mathf.Sqrt((R * R) - (ma * ma));
                w = 1 - (r / R);
                //Debug.Log("le poid est de " + w);
                //Debug.DrawLine(start, start + Vector3.down, Color.blue);
                //Debug.DrawLine(end, end + Vector3.down, Color.green);


                //Canvas :------------------------------------------------------------------------------------
                if (lecanvas.GetComponent<CanvasGroup>().alpha != 1)
                {
                    lecanvas.GetComponent<CanvasGroup>().alpha = 1f;
                }
                lecanvas.GetComponent<RectTransform>().position = start;

                //GENETIC ALGO : star function cost-----------------------------------------------------------------------
                star = (int)(w * 5) + 1;
                if (hit.collider.GetComponent<InfoSphere>().tap == true)
                {
                    hit.collider.GetComponent<InfoSphere>().nstar = star;
                    hit.collider.GetComponent<InfoSphere>().tap = false;
                }
                starec = hit.collider.GetComponent<InfoSphere>().nstar;

                //Music + Alpha-------------------------------------------------------------------------------------------
                n = hit.collider.GetComponent<InfoSphere>().presetno;
                if (w != 0 && n != m)
                {
                    StartCoroutine(ChangeMusic());
                    if (onerequest == true)
                    {
                        m = n;
                        foreach (var item in allsphere)
                        {
                            item.GetComponentInChildren<Renderer>().material.color = changeAlpha(item.GetComponentInChildren<Renderer>().material.color, 0.1f);
                        }
                        onerequest = false;
                    }
                    if (onerequest == false)
                    {
                        m = n;
                        StartCoroutine(FadeTo(hit.collider, 1.0f, 0.5f));
                        onerequest = true;
                    }
                }
            }
            else
            {
                w = 0;
                star = 0;
                lecanvas.GetComponent<CanvasGroup>().alpha = 0f;
                n = silence; // celui de mon ''Preset Silence''
                StartCoroutine(ChangeMusic());
                if (onerequest == true)
                {
                    foreach (var item in allsphere)
                    {
                        item.GetComponentInChildren<Renderer>().material.color = changeAlpha(item.GetComponentInChildren<Renderer>().material.color, 0.1f);
                    }
                    onerequest = false;
                }
            }

            //----------------------------------------------------------------------------------------------------
            //---------------------------------GENETIC ALGORITHM--------------------------------------------------
            //----------------------------------------------------------------------------------------------------
            if (Physics.Raycast(origin, direction, out hit) && hit.collider.GetType() != typeof(SphereCollider))
                goGenetic = true;
            else
                goGenetic = false;
            //Si je clique sur DNA => Algo Genetic :

            //if (GameObject.Find("dna").GetComponent<ActiveGenetic>().act == true && goGenetic)
            if (Input.GetKeyDown(KeyCode.S))
            {

                //Selection naturelle : Seuillage/Tresholding-------------------
                nokeep = 0;
                for (int i = 1; i <= popini + 1; i++) //popini sinon allcount empêche la re-iteration du seuillage //+1 pour les noms car on a enlevé 4
                {
                    if (i != silence && GameObject.Find("SpherePreset_" + i)) //preset silence degagé + Je find mes objects
                    {
                        //je verifie que la pop initiale n'a pas été liquidié totalement avec un seuil de survie de 1/2
                        //et je dégage les spheres qui ont moins de 3 étoiles
                        if (allsphere.Count > ((popini / 2)) && GameObject.Find("SpherePreset_" + i).GetComponent<InfoSphere>().nstar <= 2)
                        {
                            allsphere.Remove(GameObject.Find("SpherePreset_" + i));
                            DestroyImmediate(GameObject.Find("SpherePreset_" + i));
                            nokeep = nokeep + 1;
                        }
                    }
                }

                //Selection Mating + Mating + Mutation------------------------------
                justparent = popini - nokeep; //reproduction qu'entre parents
                StartCoroutine(MatingSaveChild(0.2f)); //Coroutine car kyma trop lent a comprendre les mess....


                var client = GetComponent<uOscClient>();
                client.Send("/preset", silence);
                //goGenetic = false;
                //GameObject.Find("DNA").GetComponent<ActiveGenetic>().act = false;
            }

        }

        //***************************************************************************************************************************//
        void OnApplicationQuit()
        {
            //Prévenir Paca que c'est terminé avec l'indice 0
            var client = GetComponent<uOscClient>();
            client.Send("/osc/respond_to", 0);
            Debug.Log("Disconnected from Pacarana");
        }
        //***************************************************************************************************************************//

        //------------------------------------
        // Méthodes utilitaires :
        //------------------------------------

        //***************************************************************************************************************************//
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

        //***************************************************************************************************************************//
        IEnumerator FadeTo(Collider coli, float aValue, float aTime)
        {
            float alpha = coli.GetComponentInChildren<Renderer>().material.color.a;
            for (float t = 0.0f; t < 1f; t += Time.deltaTime / aTime)
            {
                Color newColor = changeAlpha(coli.GetComponentInChildren<Renderer>().material.color, Mathf.Lerp(alpha, aValue, t));
                coli.GetComponentInChildren<Renderer>().material.color = newColor;
                yield return null;
            }

        }

        //***************************************************************************************************************************//
        Color changeAlpha(Color color, float newAlpha)
        {
            color.a = newAlpha;
            return color;
        }

        //***************************************************************************************************************************//
        void JsonEachWidget()
        {
            var client = GetComponent<uOscClient>();
            var bundleWidget = new Bundle(Timestamp.Now);

            for (int i = 0; i < nbwidg; i++)
            {
                bundleWidget.Add(new Message("/osc/widget", i));
            }
            client.Send(bundleWidget);
        }

        //***************************************************************************************************************************//
        //Cette méthode permet de recevoir les info (blob) de chaque preset
        //en tant normal il ne faudrait pas procéder de cette manière mais envoyer un bundle
        //le soucis c'est que kyma ne s'en sort pas avec le bundle et n'envoie pas tout les blob mais receptionne bien le bundle.
        IEnumerator WaitPleaseSendBlob(float t)
        {
            var client = GetComponent<uOscClient>();
            for (int i = 1; i <= presetnumber; ++i)
            {
                if (i != silence) //preset silence degagé
                {
                    var bundleWidget = new Bundle(Timestamp.Now);
                    foreach (int item in listID) //premier par exemple (initialisé par Json)
                    {
                        bundleWidget.Add(new Message("/vcs", (int)item, 0.12345f)); //envoie d'une valeur hasardeuse pour creer un changement lorsque je reçois blob
                    }
                    client.Send(bundleWidget); //creation du changement pour chaque preset
                    presetNoCorrect = i;
                    client.Send("/preset", i);
                    yield return new WaitForSeconds(t);
                }
            }
        }
        //***************************************************************************************************************************//
        void CorrectValue(int eventID, float valueWidget, int x)
        {
            if (x >= silence + 1) //pour le preset silence éviter dans une autre boucle, creer probleme ici
            {
                x = x - 2;
            }
            else
            {
                x = x - 1; // car liste commence à preset 0
            }

            //intégration de nos valeurs 
            for (int i = 0; i < nbwidgclean; i++)
            {
                if (allsphere[x].GetComponent<InfoSphere>().widgetValue[i].x == eventID)
                {
                    Vector2 tempo = new Vector2((float)eventID, valueWidget);
                    allsphere[x].GetComponent<InfoSphere>().widgetValue[i] = tempo;
                }
            }

        }
        //***************************************************************************************************************************//
        IEnumerator MatingSaveChild(float t)
        {
            while (popini != allsphere.Count)
            {
                int male = Random.Range(0, justparent); // dernier element non compris vu que 0 start
                int female = Random.Range(0, justparent);
                while (male == female) //pour pas le même chrosome en couple
                {
                    female = Random.Range(0, justparent);
                }
                GameObject parent1 = allsphere[male];
                GameObject parent2 = allsphere[female];
                for (int i = 1; i <= popini + 1; i++)
                {
                    if (i != silence && !GameObject.Find("SpherePreset_" + i)) //preset silence degagé + vérifie si aucun même nom existe
                    {
                        childName = "SpherePreset_" + i;//rempli les trou de i sphere 
                        childpresetno++;
                        break;
                    }
                }

                SphereMusicClone = Instantiate(SphereMusic) as GameObject;
                SphereMusicClone.transform.position = new Vector3(this.transform.position.x + Random.Range(-1f, 1f), this.transform.position.y + Random.Range(-1f, 1f), this.transform.position.z + Random.Range(-0.3f, 1f));
                SphereMusicClone.transform.Translate(new Vector3(0, 0.3f, 3f));
                Color mergeColor = (parent1.GetComponentInChildren<Renderer>().material.color + parent2.GetComponentInChildren<Renderer>().material.color) / 2;
                SphereMusicClone.GetComponentInChildren<Renderer>().material.color = changeAlpha(mergeColor, 0.3f); //9 preset max
                SphereMusicClone.name = childName;
                InfoSphere scriptvaleur = SphereMusicClone.GetComponent<InfoSphere>();
                scriptvaleur.presetno = childpresetno;
                allsphere.Add(SphereMusicClone); //pas dans le bon ordre vu qu'on ajoute en dernier dans la liste (contenant deja les anciens) => Attention !!

                //faire random avant... parent1 et 2-----------------
                int crossing = Random.Range(1, 2);
                //si crossing = 1, alors crossing normal
                GameObject p1 = parent1;
                GameObject p2 = parent2;
                //sinon si crossing = 2, alors crossing inverse
                if (crossing == 2)
                {
                    p2 = parent1;
                    p1 = parent2;
                }

                for (int i = 0; i < p1.GetComponent<InfoSphere>().widgetValue.Count / 2; i++)
                {
                    scriptvaleur.widgetValue.Add(p1.GetComponent<InfoSphere>().widgetValue[i]);
                    //Debug.Log(" x : " + p1.GetComponent<InfoSphere>().widgetValue[i].x + " et y :" + p1.GetComponent<InfoSphere>().widgetValue[i].y);
                }
                for (int i = p1.GetComponent<InfoSphere>().widgetValue.Count / 2; i < p2.GetComponent<InfoSphere>().widgetValue.Count; i++)
                {
                    scriptvaleur.widgetValue.Add(p2.GetComponent<InfoSphere>().widgetValue[i]);
                    //Debug.Log(" x2 : " + p2.GetComponent<InfoSphere>().widgetValue[i].x + " et y2 :" + p2.GetComponent<InfoSphere>().widgetValue[i].y);
                }

                Debug.Log(SphereMusicClone.name + " = " + allsphere[male].name + " + " + allsphere[female].name);

                //Création du preset enfant sur kyma--------------------------------------
                //on se place sur un preset parent pour save
                var client = GetComponent<uOscClient>();
                //client.Send("/preset", 8); //car kyma commence par preset 1 // PROBLEME ?????????????????? VU LA POSITION DU PRESET

                var bundle1 = new Bundle(Timestamp.Now);
                foreach (var item in scriptvaleur.widgetValue)
                {
                    int eventKyma = (int)item.x;
                    float valueKyma = item.y;
                    bundle1.Add(new Message("/vcs", eventKyma, valueKyma));
                    //Debug.Log("vcs child : " + eventKyma + " et value " + valueKyma);
                }
                client.Send(bundle1);
                Debug.Log("sauvegarde...");
                client.Send("/preset", 130); //creation d'un new preset

                //Mutations Elitisme : sur les 3 etoiles(une chance sur 2) si existe-------------------
                //mutation...
                //Pour cela, utilisation de générateur Random paire(X, X) ligne colonne parmis les chromosomes 3 etoiles

                yield return new WaitForSeconds(t);
            }
        }

    }

    // GENETIC ALGORITHME :
    //------------------------ 
    /* RMQ : attention le nombre aléatoire seed si ça ne reutilise pas le meme nbr chaque fois...
     * Faudrait verifier si on sait le nombre de widget d'un preset... pour généraliser
     * 
    1) fonction cout = stars  et  variables = widget  et  il y a nwidget = nombre de gênes
       Donc plus il y a d'étoiles, plus le cout est faible et donc le preset est bien !
       => Encodage des variables devrait être fait par rapport au pourcentage du slider donc variable/maxVar pour chaque widget !!! ... Decodage sens inverse puis save sur kyma
    2) population initial composé de presetnumber chromosomes qui sont composé de nwidget gênes // VOIR MESSAGE OSC POUR RECEVOIR LES VARIABLES ET EN FAIRE DES CHROMOSOMES
    3) Selection naturelle : Seuillage/Tresholding pour degager ceux en dessous de 3 etoiles tout en faisant attention à pas liquidier la totalité de la population
    4) Selection Mating : au hasard sur les keep 
    5) Mating : retour à pop initiale avec 2 parents qui donne deux enfants 
       donc selection d'un kinetochore sur notre chromosome encodé (ex : 0.7,0.8,0.3 // 0.1,1,0.5,0.8) où // est le kinetochore ou point de croisement 
    6) Mutations Elitisme : sur les 3 etoiles (une chance sur 2) si il y en a ! Pour cela, utilisation de générateur Random paire ( X, X) ligne colonne parmis les chromosomes 3 etoiles
       Rmq : pas de mutation sur iteration final... donc attention car c'est le gars qui itere => A voir
    7) FIN : Convergence en fonction du besoin et des gouts de l'utilisateur
    */
}