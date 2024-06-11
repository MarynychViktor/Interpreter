namespace Interpreter;

public class Scanner
{
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();
    private int _current;
    private int _start;
    private int _line = 1;
    private static readonly Dictionary<string, TokenType> _keywords;


    static Scanner()
    {
        _keywords = new Dictionary<string, TokenType>();
        _keywords.Add("and", TokenType.AND);
        _keywords.Add("class", TokenType.CLASS);
        _keywords.Add("else", TokenType.ELSE);
        _keywords.Add("false", TokenType.FALSE);
        _keywords.Add("for", TokenType.FOR);
        _keywords.Add("fun", TokenType.FUN);
        _keywords.Add("if", TokenType.IF);
        _keywords.Add("nil", TokenType.NIL);
        _keywords.Add("or", TokenType.OR);
        _keywords.Add("print", TokenType.PRINT);
        _keywords.Add("return", TokenType.RETURN);
        _keywords.Add("super", TokenType.SUPER);
        _keywords.Add("this", TokenType.THIS);
        _keywords.Add("true", TokenType.TRUE);
        _keywords.Add("var", TokenType.VAR);
        _keywords.Add("while", TokenType.WHILE);
    }

    public Scanner(string source)
    {
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    public void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case '-':
                AddToken(TokenType.MINUS);
                break;
            case '+':
                AddToken(TokenType.PLUS);
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case '*':
                AddToken(TokenType.STAR);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && IsAtEnd())
                    {
                        Advance();
                    }
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;
            case '"':
                String();
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                    break;
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                    break;
                }
                else
                {
                    Cslox.Error(_line, "Unexpected character.");
                    break;
                }
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var text = _source.Substring(_start, _current - _start);
        AddToken(_keywords.ContainsKey(text) ? _keywords[text] : TokenType.IDENTIFIER);
    }

    private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER, Double.Parse(_source.Substring(_start, _current - _start)));
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Cslox.Error(_line, "Unterminated string.");
            return;
        }

        Advance();

        var value = _source.Substring(_start + 1, _current - _start - 1);
        AddToken(TokenType.STRING, value);
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private bool Match(char ch)
    {
        if (IsAtEnd())
        {
            return false;
        }

        if (_source[_current] != ch) return false;
        _current++;
        return true;
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, Object literal)
    {
        var text = _source.Substring(_start, _current  - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private char Advance() => _source[_current++];

    private bool IsAtEnd() => _current >= _source.Length;
}