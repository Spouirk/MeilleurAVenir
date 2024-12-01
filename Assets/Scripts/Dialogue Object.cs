using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;

public class DialogueObject {
    private const string kStart = "START";
    private const string kEnd = "END";

    public struct Response {
        public string displayText;
        public string destinationNode;

        public Response( string display, string destination ) {
            displayText = display;
            destinationNode = destination;
        }
    }

    public class Node {
        public string title;
        public string text;
        public string tone;
        public List<string> tags;
        public List<Response> responses;

        internal bool IsEndNode() {
            return tags.Contains( kEnd );
        }

        internal bool IsVoyanteNode() {
            return tags.Contains( "VOYANTE" );
        }
        
        internal bool IsPNJNode() {
            return tags.Contains( "PNJ" );
        }

        internal bool IsQuestion() {
            return tags.Contains( "QUESTION" );
        }

        internal bool isDescription() {
            return tags.Contains( "DESCRIPTION" );
        }

        internal bool isFade() {
            return tags.Contains( "FADE" );
        }

        internal bool IsNewClient() {
            return tags.Contains( "NEWCLIENT" );
        }

        internal bool IsNewWeek() {
            return tags.Contains( "NEWWEEK" );
        }

        internal bool IsInverse() {
            return tags.Contains( "INVERSE" );
        }

        internal bool IsEndOfWeek() {
            return tags.Contains( "ENDOFWEEK" );
        }

        // TODO proper override
        public string Print() {
            return "";//string.Format( "Node {  Title: '%s',  Tag: '%s',  Text: '%s'}", title, tag, text );
        }

        private enum COMMAND_TYPE
        {
            SET,
            IF,
            ELSEIF,
            ELSE,
            SOUND,
            DEFAULT
        }

        private enum SOUND_TYPE
        {
            ALEX_ENTREE,
            ALEX_DUBITATIF,
            ALEX_DESESPERE,
            ALEX_ENTHOUSIASTE,
            CASSANDRE_ENTREE,
            CASSANDRE_CONFIANTE,
            CASSANDRE_PROVOCATRICE,
            CASSANDRE_DEDAIGNEUSE,
            MELVIN_ENTREE,
            MELVIN_AGACE,
            MELVIN_DEDAUGNEUX
        }

        public string GetText()
        {
            Regex patternExtract = new Regex(@"\(\((?<commande>.*)\)\)(.*\[(?<text>.*)\]|)");
            bool isConditionAlreadyValide = false;

            string parsedText = "";

            foreach(string line in text.Split("\n"))
            {
                Match match = patternExtract.Match(line);
                if (match.Success){
                    COMMAND_TYPE commandType = GetCommandType(match.Groups["commande"].Value);
                    switch (commandType)
                    {
                        case COMMAND_TYPE.SET:
                            Regex patternExtractSetVariable = new Regex(@"set: \$(?<variableName>.*) to \'(?<value>.*)'");
                            Match variableMatch = patternExtractSetVariable.Match(match.Groups["commande"].Value);

                            CardManager.Instance.SetVariable(variableMatch.Groups["variableName"].Value, variableMatch.Groups["value"].Value);
                            break;
                        case COMMAND_TYPE.IF:
                            isConditionAlreadyValide = false ;

                            if (testCondition(match.Groups["commande"].Value))
                            {
                                isConditionAlreadyValide = true ;
                                if (parsedText.Length != 0) parsedText += "\n";
                                parsedText +=  match.Groups["text"].Value;
                            }

                            break;
                        case COMMAND_TYPE.ELSEIF:
                            if (isConditionAlreadyValide) break;

                            if (testCondition(match.Groups["commande"].Value))
                            {
                                isConditionAlreadyValide = true;
                                if (parsedText.Length != 0) parsedText += "\n";
                                parsedText += match.Groups["text"].Value;
                            }
                            break;
                        case COMMAND_TYPE.ELSE:
                            if (isConditionAlreadyValide) break;
                            if (parsedText.Length != 0) parsedText += "\n";
                            parsedText += match.Groups["text"].Value;
                            break;
                        case COMMAND_TYPE.SOUND:
                            playSound(match.Groups["commande"].Value);
                            break;
                        case COMMAND_TYPE.DEFAULT:
                            break;
                    }
                }
                else parsedText += parsedText.Length == 0 ? line : ("\n" + line);
            }

            return parsedText;
        }

        private bool testCondition(string condition)
        {
            Regex patternExtractIf = new Regex(@"(else-if|if): \$(?<variableName>.*) is \'(?<value>.*)\'");
            Match conditionIfMatch = patternExtractIf.Match(condition);

            Debug.Log(condition);
            string conditionVariableValue = CardManager.Instance.GetVariable(conditionIfMatch.Groups["variableName"].Value);

            return conditionVariableValue == conditionIfMatch.Groups["value"].Value;
        }

        private void playSound(string sound)
        {
            Regex patternSound = new Regex(@"(sound): (?<soundType>.*)");
            Match soundMatch = patternSound.Match(sound);

            string soundType = soundMatch.Groups["type"].Value;
            Debug.Log(soundType);
        }

        private COMMAND_TYPE GetCommandType(string command)
        { 
            if(command.StartsWith("set:")) return COMMAND_TYPE.SET;
            if(command.StartsWith("if")) return COMMAND_TYPE.IF;
            if(command.StartsWith("else-if")) return COMMAND_TYPE.ELSEIF;
            if(command.StartsWith("else")) return COMMAND_TYPE.ELSE;
            if(command.StartsWith("sound")) return COMMAND_TYPE.SOUND;
            return COMMAND_TYPE.DEFAULT;
        }

    }

    public class Dialogue {
        string title;
        Dictionary<string, Node> nodes;
        string titleOfStartNode;
        public Dialogue( TextAsset twineText ) {
            nodes = new Dictionary<string, Node>();
            ParseTwineText( twineText.text );
        }

        public Node GetNode( string nodeTitle ) {
            return nodes [ nodeTitle ];
        }

        public Node GetStartNode() {
            UnityEngine.Assertions.Assert.IsNotNull( titleOfStartNode );
            return nodes [ titleOfStartNode ];
        }

        public void ParseTwineText( string twineText )
        {
            string[] nodeData = twineText.Split(new string[] { "::" }, StringSplitOptions.None);
 
            bool passedHeader = false;
            // const int kIndexOfContentStart = 4;
            for ( int i = 0; i < nodeData.Length; i++ )
            {
 
                // The first node comes after the UserStylesheet node
                if ( !passedHeader )
                {
                    if ( nodeData[ i ].StartsWith( " UserStylesheet" ) )
                        passedHeader = true;
 
                    continue;
                }
 
                // Note: tags are optional
                // Normal Format: "NodeTitle [Tags, comma, seperated] \r\n Message Text \r\n [[Response One]] \r\n [[Response Two]]"
                // No-Tag Format: "NodeTitle \r\n Message Text \r\n [[Response One]] \r\n [[Response Two]]"
                string currLineText = nodeData[i];
 
                // Remove position data
                int posBegin = currLineText.IndexOf("{\"position");
                if ( posBegin != -1 )
                {
                    int posEnd = currLineText.IndexOf("}", posBegin);
                    currLineText = currLineText.Substring( 0, posBegin ) + currLineText.Substring( posEnd + 1 );
                }
 
                bool tagsPresent = currLineText.IndexOf( "[" ) < currLineText.IndexOf( "\r\n" );
                int endOfFirstLine = currLineText.IndexOf( "\r\n" );
 
                int startOfResponses = -1;
                int startOfResponseDestinations = currLineText.IndexOf( "[[" );
                bool lastNode = (startOfResponseDestinations == -1);
                if ( lastNode )
                    startOfResponses = currLineText.Length;
                else
                {
                    // Last new line before "[["
                    startOfResponses = currLineText.Substring( 0, startOfResponseDestinations ).LastIndexOf( "\r\n" );
                }
 
                // Extract Title
                int titleStart = 0;
                int titleEnd = tagsPresent
                    ? currLineText.IndexOf( "[" )
                    : endOfFirstLine;
                UnityEngine.Assertions.Assert.IsTrue( titleEnd > 0, "Maybe you have a node with no responses?" );
                string title = currLineText.Substring(titleStart, titleEnd).Trim();
 
                // Extract Tags (if any)
                string tags = tagsPresent
                    ? currLineText.Substring( titleEnd + 1, (endOfFirstLine - titleEnd)-2)
                    : "";
 
                if ( !string.IsNullOrEmpty(tags) && tags[ tags.Length - 1 ] == ']' )
                    tags = tags.Substring( 0, tags.Length - 1 );
 
                // Extract Message Text & Responses
                string messsageText = currLineText.Substring( endOfFirstLine, startOfResponses - endOfFirstLine).Trim();
                string responseText = currLineText.Substring( startOfResponses ).Trim();
 
                Node curNode = new Node();
                curNode.title = title;
                curNode.text = messsageText;
                curNode.tags = new List<string>( tags.Split( new string[] { " " }, StringSplitOptions.None ) );
 
                if ( curNode.tags.Contains( kStart ) )
                {
                    UnityEngine.Assertions.Assert.IsTrue( null == titleOfStartNode );
                    titleOfStartNode = curNode.title;
                }
 
                // Note: response messages are optional (if no message then destination is the message)
                // With Message Format: "\r\n Message[[Response One]]"
                // Message-less Format: "\r\n [[Response One]]"
                curNode.responses = new List<Response>();
                if ( !lastNode )
                {
                    List<string> responseData = new List<string>(responseText.Split( new string [] { "\r\n" }, StringSplitOptions.None ));
                    for ( int k = responseData.Count - 1; k >= 0; k-- )
                    {
                        string curResponseData = responseData[k];
 
                        if ( string.IsNullOrEmpty( curResponseData ) )
                        {
                            responseData.RemoveAt( k );
                            continue;
                        }
 
                        Response curResponse = new Response();
                        int destinationStart = curResponseData.IndexOf( "[[" );
                        int destinationEnd = curResponseData.IndexOf( "]]" );
                        UnityEngine.Assertions.Assert.IsFalse( destinationStart == -1, "No destination around in node titled, '" + curNode.title + "'" );
                        UnityEngine.Assertions.Assert.IsFalse( destinationEnd == -1, "No destination around in node titled, '" + curNode.title + "'" );
                        string destination = curResponseData.Substring(destinationStart + 2, (destinationEnd - destinationStart)-2);
                        curResponse.destinationNode = destination;
                        if ( destinationStart == 0 )
                            curResponse.displayText = ""; // If message-less, then message is an empty string
                        else
                            curResponse.displayText = curResponseData.Substring( 0, destinationStart );
                        curNode.responses.Add( curResponse );
                    }
                }
 
                nodes[ curNode.title ] = curNode;
            }
        }
    }
}

