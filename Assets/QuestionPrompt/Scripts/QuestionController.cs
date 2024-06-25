using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QuestionController : MonoBehaviour
{
    [Tooltip("A JSON resource containing question data.")]
    public TextAsset QuestionsData;

    #region Type Definitions

    [Serializable]
    public enum QuestionDifficulty
    {
        Beginner,
        Intermediate,
        Advanced
    }
    
    [Serializable]
    public enum QuestionType
    {
        Type,
        Select
    }

    [Serializable]
    public enum QuestionCategory
    {
        Grammar,
        Punctuation
    }

    [Serializable]
    public class QuestionList
    {
        public Question[] questions;
    }

    [Serializable]
    public class Question
    {
        public string questionText;
        public QuestionType type;
        public QuestionCategory category;
        public QuestionDifficulty difficulty;
        public Answer[] answers;
    }

    [Serializable]
    public class Answer
    {
        public string answerText;
        public bool isCorrect;
    }

    #endregion

    private List<Question> questions;
    private Question activeQuestion
    {
        get { return activeQuestion; }
        set
        {
            // Update the visuals whenever the question is changed
            SetPromptHeader(value);
            SetAnswers(value);
        }
    }

    void Start()
    {
        questions = DeserializeQuestions(QuestionsData.text);
        activeQuestion = GetRandomQuestion();

        Debug.Log(questions.Count);

        // Remove this later; used for debugging
        StartCoroutine(CycleButtons());
        IEnumerator CycleButtons()
        {
            while (true)
            {
                Debug.Log("Cycling buttons");
                activeQuestion = GetRandomQuestion();
                yield return new WaitForSecondsRealtime(3);
            }
        }
    }

    private Question GetRandomQuestion()
    {
        /* The use of UnityEngine.Random over System.Random is intentional to minimise complexity
         * of the implementation that would be necessary to generate numbers that aren't frequently
         * repeated with System.Random due to new Random instances being initialised using the clock
         * (https://stackoverflow.com/questions/767999/random-number-generator-only-generating-one-random-number) */
        return questions[UnityEngine.Random.Range(0, questions.Count)];
    }

    private void OnAnswerSelected()
    {
        throw new NotImplementedException();
    }

    #region Visuals

    /// <summary>
    /// Set the contents of the text components in the prompt header.
    /// </summary>
    /// <param name="question"></param>
    private void SetPromptHeader(Question question)
    {
        string parentPath = $"{gameObject.name}/Header/Text";

        GameObject.Find($"{parentPath}/Title")
                  .GetComponent<TextMeshPro>()
                  .SetText($"{question.category} ({question.difficulty})");

        GameObject.Find($"{parentPath}/Subtitle")
                  .GetComponent<TextMeshPro>()
                  .SetText($"{question.type}");

        GameObject.Find($"{parentPath}/Question")
                  .GetComponent<TextMeshPro>()
                  .SetText(question.questionText);
    }

    /// <summary>
    /// Set the text of the answer buttons in the prompt.
    /// </summary>
    /// <param name="question"></param>
    private void SetAnswers(Question question)
    {
        List<Answer> validAnswers = new(question.answers);

        // The TMP component of each button
        List<TextMeshPro> validButtonTexts = GameObject
            .Find($"{gameObject.name}/Answers")
            .GetComponentsInChildren<TextMeshPro>()
            .ToList();

        foreach (TextMeshPro buttonText in validButtonTexts)
        {
            /* Randomise the position of each Answer
             * (see GetRandomQuestion() about UnityEngine.Random) */
            int randNum = UnityEngine.Random.Range(0, validAnswers.Count);
            Answer selAnswer = validAnswers[randNum];

            buttonText.SetText($"{selAnswer.answerText} ({selAnswer.isCorrect})");

            // Remove the Answer to prevent assigning it again
            validAnswers.Remove(selAnswer);
        }
    }

    #endregion

    #region Deserialization

    /// <summary>
    /// Get a list of the deserialized questions.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private List<Question> DeserializeQuestions(string json)
    {
        // Newtonsoft.Json is used instead of Unity's JsonUtility as the latter would
        // consistently deserialise enums into the first value, regardless of the string value
        QuestionList questionList = JsonConvert.DeserializeObject<QuestionList>(json);
        var questions = questionList.questions.ToList();
        ValidateQuestions(questions);

        return questions;
    }

    /// <summary>
    /// Validate that the questions contain the expected data.
    /// </summary>
    /// <param name="questions"></param>
    /// <exception cref="Exception"></exception>
    private void ValidateQuestions(List<Question> questions)
    {
        for (int qIndex = 0; qIndex < questions.Count; qIndex++)
        {
            Question question = questions[qIndex];
            if (question.answers.Count() != 3)
            {
                throw new Exception($"Index {qIndex}: A question must have exactly three answers.");
            }
            else if (question.answers.Count(ans => ans.isCorrect) != 1)
            {
                throw new Exception($"Index {qIndex}: A question must have exactly one correct answer.");
            }
        }
    }

    #endregion
}
