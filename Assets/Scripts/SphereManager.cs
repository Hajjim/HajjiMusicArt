using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
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

        public int inPort = 8000;
        public GameObject SphereMusic;
        GameObject SphereMusicClone;
        //Collider lastcollider;
        bool onerequest = false;
        bool goGenetic = false;

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
        public int silence = 4;

        List<GameObject> allsphere;
        private int popini;
        private string childName;
        private int childpresetno;
        private int nokeep;

        private int nbwidg;
        public int presetnumber = 1;
        //private int valeur;
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
        // Méthodes :
        //------------------------------------

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
            n = silence;
            StartCoroutine(ChangeMusic());

            //recuperation des widgets  // MAIS SUR UN PRESET ???? JE VEUX TOUT LES PRESET ET LEUR VALUE
            var bundleWidget = new Bundle(Timestamp.Now);
            for (int i = 0; i < nbwidg; i++)
            {
                bundleWidget.Add(new Message("/osc/widget", i));
            }
            client.Send(bundleWidget);

        }



        void FixedUpdate()
        {

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

            origin = GameObject.Find("MixedRealityCamera").transform.position;//la pos sur lequel raycast se trouve
            direction = GameObject.Find("MixedRealityCamera").transform.forward; //la direction du vecteur 
            //Raycast:----------------------------------------------------------------------------------------
            Ray ray = new Ray(origin, direction); //raycast
                                                  // RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance); //pour toucher à longue portée toutes mes spheres
            Debug.DrawLine(origin, GameObject.Find("MixedRealityCamera").transform.position + direction * maxRayDistance, Color.red); //dessin mon raycast sur la scène:

            //foreach (RaycastHit hit in hits) // si je veux calculer pour toutes les spheres touchées
            RaycastHit hit; //pour le solo
            if (Physics.Raycast(origin, direction, out hit) && hit.collider.GetType() == typeof(SphereCollider))
            {
                //Calcul du poids:------------------------------------------------------------------------------------
                sphere = (SphereCollider)hit.collider;
                //Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.cyan);  //j'affiche une verticale sur intersection touchée       
                VecHypo = sphere.transform.position - ray.origin; //position sphere (centre) - point dd'origine camera Raycast = Hypo vecteur
                //Hypo = Vector3.Dot(VecHypo, VecHypo); //hypo produit vectoriel
                Adj = Vector3.Dot(VecHypo, direction); //Adjacente (hypo projeté) sur là où je regarde car hypo par rapport au centre
                //oppose = Mathf.Sqrt((Hypo * Hypo) - (Adj * Adj)); //opposé
                dist = Adj - hit.distance; //distance entre là où je regarde (jusque centre projeté parallele) et là où je frappe avec hit
                t0 = Adj - dist;
                //t1 = Adj + dist;
                start = ray.origin + direction * t0;
                //end = ray.origin + direction * t1;
                ma = dist; //car joue le role de notre adj par rapport à moyenne de point t0 et t1
                //R = (float) (sphere.radius*1.4 / 2); //car radius de sphere du collider = diamètre et 1.4 pour scale look
                R = (float)(sphere.radius / 2);
                r = Mathf.Sqrt((R * R) - (ma * ma));
                w = 1 - (r / R);
                //Debug.Log("le poid est de " + w);
                //Debug.DrawLine(start, start + Vector3.down, Color.blue);
                //Debug.DrawLine(end, end + Vector3.down, Color.green);

                //MessageOSC + Scale :------------------------------------------------------------------------------------
                n = hit.collider.GetComponent<InfoSphere>().presetno;
                if (lecanvas.GetComponent<CanvasGroup>().alpha != 1)
                {
                    lecanvas.GetComponent<CanvasGroup>().alpha = 1f;
                }
                lecanvas.GetComponent<RectTransform>().position = start;
                //star function cost-----------------------------------------------------------------------------------------------------
                star = (int)(w * 5) + 1;
                if (hit.collider.GetComponent<InfoSphere>().tap == true)  // A remplacer avec IInputClickHandler ou IInputHandler  ?
                {
                    hit.collider.GetComponent<InfoSphere>().nstar = star;
                    hit.collider.GetComponent<InfoSphere>().tap = false;
                }
                starec = hit.collider.GetComponent<InfoSphere>().nstar;

                //Music + Alpha--------------------------------------------------------------------------------------------------
                if (w != 0 && n != m)
                {
                    StartCoroutine(ChangeMusic());
                    if (onerequest == true)
                    {

                        m = n;
                        //lastcollider.GetComponent<Renderer>().transform.localScale = lastcollider.GetComponent<Renderer>().transform.localScale / 1.4f;
                        //lastcollider.GetComponentInChildren<Renderer>().material.color = changeAlpha(lastcollider.GetComponentInChildren<Renderer>().material.color, 0.1f);
                        foreach (var item in allsphere)
                        {
                            item.GetComponentInChildren<Renderer>().material.color = changeAlpha(item.GetComponentInChildren<Renderer>().material.color, 0.1f);
                        }
                        onerequest = false;
                        onerequest = false;

                    }
                    if (onerequest == false)
                    {
                        m = n;
                        //hit.collider.GetComponent<Renderer>().transform.localScale = hit.collider.GetComponent<Renderer>().transform.localScale * 1.4f;              
                        //hit.collider.GetComponentInChildren<Renderer>().material.color = changeAlpha(hit.collider.GetComponentInChildren<Renderer>().material.color, 0.8f);
                        StartCoroutine(FadeTo(hit.collider, 1.0f, 0.5f));
                        onerequest = true;
                        //lastcollider = hit.collider;

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
                    //lastcollider.GetComponent<Renderer>().transform.localScale = lastcollider.GetComponent<Renderer>().transform.localScale / 1.4f;
                    //lastcollider.GetComponentInChildren<Renderer>().material.color = changeAlpha(lastcollider.GetComponentInChildren<Renderer>().material.color, 0.1f);
                    foreach (var item in allsphere)
                    {
                        item.GetComponentInChildren<Renderer>().material.color = changeAlpha(item.GetComponentInChildren<Renderer>().material.color, 0.1f);
                    }
                    onerequest = false;
                }
            }



            //GENETIC ALGO-----------------------------------------------------------------------------------------
            //Vérification si c'est un boxcollider (DNA)
            if (Physics.Raycast(origin, direction, out hit) && hit.collider.GetType() != typeof(SphereCollider))
            {
                goGenetic = true; //Ok verif réussie
            }
            else
            {
                goGenetic = false; //échoue
            }
            //Si je clique sur DNA => Algo Genetic
            if (GameObject.Find("DNA").GetComponent<GeneticActivate>().DNA == true && goGenetic)
            {

                //Selection naturelle : Seuillage/Tresholding-------------------
                nokeep = 0;
                for (int i = 1; i <= popini + 1; i++) //popini sinon allcount empêche la re-iteration du seuillage //+1 pour les noms car on a enlevé 4
                {
                    if (i != silence && GameObject.Find("SpherePreset_" + i)) //preset silence degagé + Je find mes objects
                    {
                        //je verifie que la pop initiale n'a pas été liquidié totalement avec un seuil de survie de 1/3
                        //et je dégage les spheres qui ont moins de 3 étoiles
                        if (allsphere.Count > ((popini / 2)) && GameObject.Find("SpherePreset_" + i).GetComponent<InfoSphere>().nstar <= 2)
                        {
                            allsphere.Remove(GameObject.Find("SpherePreset_" + i));
                            DestroyImmediate(GameObject.Find("SpherePreset_" + i));
                            nokeep = nokeep + 1;
                        }
                    }
                }

                //Selection Mating + Mating : au hasard sur les keep-------------------
                int justparent = popini - nokeep; //reproduction qu'entre parents
                Debug.Log(justparent);
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
                    allsphere.Add(SphereMusicClone); //pas dans le bon ordre vu qu'on ajoute en dernier => Attention !!

                    //parents 1 Split + parents 2 split = encodage this sphere !!!!!!!!!!!!!!!!!!!! OU AU HASARD INVERSE 1 et 2
                    //parents 2 Split + Parents 1 split = encodage this second sphere !!!!!!!!!!!!!??????
                    //puis on modifie et save sur le preset ecrase n° childpresetno 
                    scriptvaleur.encodage = allsphere[male].GetComponent<InfoSphere>().encodage + allsphere[female].GetComponent<InfoSphere>().encodage;
                    Debug.Log(SphereMusicClone.name + " = " + allsphere[male].name + " + " + allsphere[female].name);
                    //Mutations Elitisme : sur les 3 etoiles(une chance sur 2) si existe-------------------
                }
                goGenetic = false;
                GameObject.Find("DNA").GetComponent<GeneticActivate>().DNA = false;
            }


            if (Input.GetKeyDown(KeyCode.L))
            {
                //mutation...
                //Pour cela, utilisation de générateur Random paire(X, X) ligne colonne parmis les chromosomes 3 etoiles
                var client = GetComponent<uOscClient>();
                var bundle1 = new Bundle(Timestamp.Now);
                bundle1.Add(new Message("/vcs", 3145730, 0.2f));
                bundle1.Add(new Message("/vcs", 3145731, 0.5f));
                bundle1.Add(new Message("/vcs", 3145732, 0.7f));

                client.Send(bundle1);
                Debug.Log(bundle1);
                //client.Send("/preset", 130); //creation d'un new preset
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


        Color changeAlpha(Color color, float newAlpha)
        {
            color.a = newAlpha;
            return color;
        }


        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1);
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
            }

            //pour le nombre de sph------------------------
            if (message.address == "/osc/notify/presets/hajji")
            {
                presetnumber = (int)valeur;
                Debug.Log(msg + " => Nombre : " + valeur);
                //Je créé le nombre de sphère requis : 
                //population initial composé de presetnumber chromosomes qui sont composé de nwidget gênes
                for (int i = 1; i <= presetnumber; i++)
                {
                    if (i != silence) //preset silence degagé
                    {
                        string objectName = "SpherePreset_" + i; // Obtenir le nom avec / preset ... etc !!! => A optimiser
                                                                 // float angleIteration = 360 / presetnumber;
                                                                 // float currentRotation = angleIteration * i;
                        SphereMusicClone = Instantiate(SphereMusic) as GameObject;
                        SphereMusicClone.transform.position = new Vector3(this.transform.position.x + Random.Range(-1f, 1f), this.transform.position.y + Random.Range(-1f, 1f), this.transform.position.z + Random.Range(-0.3f, 1f));
                        // SphereMusicClone.transform.rotation = this.transform.rotation;
                        // SphereMusicClone.transform.Rotate(new Vector3(0, currentRotation, 0));
                        SphereMusicClone.transform.Translate(new Vector3(0, 0.3f, 3f));
                        SphereMusicClone.GetComponentInChildren<Renderer>().material.color = changeAlpha(mycolors.Pop(), 0.3f); //9 preset max
                        SphereMusicClone.name = objectName;
                        InfoSphere scriptvaleur = SphereMusicClone.GetComponent<InfoSphere>();
                        scriptvaleur.presetno = i;
                        allsphere.Add(SphereMusicClone);

                    }
                }
                popini = allsphere.Count;
                childpresetno = popini + 1; //+1 car silence pas compté dans popini
            }

            // After bundle que j'ai send avec /osc/widget, i

            if (message.address == "/osc/widget") //value[0] = index of widget and value[1] = JSON String
            {

                var jsonstring = (string)message.values[1];
                // Debug.Log(jsonstring);

                Rootobject json = JsonConvert.DeserializeObject<Rootobject>(jsonstring);

                if (json != null && json.label != "Trigger" && json.concreteEventID != 0)
                {
                    foreach (var item in allsphere)
                    {
                        item.GetComponent<InfoSphere>().widgetValue.Add(json.concreteEventID, json.label);
                        Debug.Log(item.GetComponent<InfoSphere>().widgetValue[json.concreteEventID]);
                    }

                }


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







/*
 * ⇒ /preset,i	index	Select the given VCS preset
Use this message to select a particular VCS preset. 

The integer argument is the one-based index into the array of VCS presets. Use 126 to select the previous preset, 127 to select the next preset, 128 to roll the dice, 129 to resend the current settings, 130 to save the current settings as a new preset.


    
⇒ /vcs,if...	eventID1, value1, ...	Change the value of one or more VCS widgets
Use this message to change the value of one or more VCS widgets, identified by EventID. An EventID is a unique number identifying the source of control and can be obtained from the concreteEventID entry in the widget information returned by the /osc/widget,i message. 

The argument is any number (up to 128) of EventID/value pairs and the number of pairs is indicated in the OSC message type. For example, to send three pairs you would use /vcs,ififif. 

Also see below for a simpler message to change the value of a widget using the name instead of the EventID.




    // OSEF ?
    ⇐ /vcs,b	{ byteCount, int_id0, float_value0, ... }	Notification of change of value of one or more VCS widgets
The Paca(rana) sends this message to your software if VCS notifications have been turned on and one or more VCS widgets have changed value. 

The blob argument contains big-endian data in the following format: 

byteCount is the size of the blob in bytes; byteCount / 8 is the number of EventID/value pairs in the blob 

int_id0, float_value0 is the 32-bit integer EventID and the 32-bit float value of the widget that changed value 

... repeat EventID and value pairs for each widget that changed value.





    ⇒ /vcs/<name>/<channel>,f	value	Change the value of the named VCS widget
Use this message to change the value of the VCS widget with the given name on the given channel. 

<name> and <channel> are placeholders for the real name and channel of the widget to be changed. For example, to change the BPM fader on channel 1, you would use the message /vcs/BPM/1. 

The float argument is the new value for the named widget. 

See this discussion for more information about this set of messages. 

The Paca(rana) replies to the sender of this message when the widget's value is changed using the VCS or other source of control of the widget (see below).

    http://www.symbolicsound.com/cgi-bin/bin/view/Learn/ProgramOSCMessages


VOIR LEXAMPLE SOURCE POUR POURCENTAGE AVEC /VCS


    */
