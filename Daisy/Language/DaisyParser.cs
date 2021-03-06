﻿using System.Collections.Generic;
using System.Linq;

namespace Ancestry.Daisy.Language
{
    using Ancestry.Daisy.Language.AST;
    using Ancestry.Daisy.Utils;

    public class DaisyParser
    {
        private readonly LookAheadStream<Token> tokenStream;

        public DaisyParser(LookAheadStream<Token> tokenStream)
        {
            this.tokenStream = tokenStream;
        }

        public DaisyAst Parse()
        {
            var root = tokenStream.MoveNext() 
                ? ParseExpression() 
                : null;
            return new DaisyAst(root);
        }

        private IDaisyAstNode ParseExpression()
        {
            var statement = ParseUnaryExpr();
            var endLoop = false;
            while(!endLoop)
            {
                switch (tokenStream.Current.Kind)
                {
                    case TokenKind.Statement:
                    case TokenKind.And:
                    case TokenKind.Not:
                        statement = ParseAnd(statement);
                        break;
                    case TokenKind.Or:
                        statement = ParseOr(statement);
                        break;
                    case TokenKind.EOF:
                    case TokenKind.EndGroup:
                        endLoop = true;
                        break;
                    default:
                        throw new UnexpectedTokenException(tokenStream.Current, new[]
                            {
                                TokenKind.Statement, TokenKind.And, TokenKind.Or,
                                TokenKind.EOF, TokenKind.StartGroup, 
                            });
                }
            }
            return statement;
        }

        private AndOperatorNode ParseAnd(IDaisyAstNode left)
        {
            if(tokenStream.Current.Kind == TokenKind.And) MoveNextGuaranteed();
            var right = ParseUnaryExpr();
            return new AndOperatorNode(left, right);
        }

        private IDaisyAstNode ParseUnaryExpr()
        {
            if(tokenStream.Current.Kind == TokenKind.Not)
            {
                MoveNextGuaranteed();
                var pred = ParsePredicate();
                return new NotOperatorNode(pred);
            }
            return ParsePredicate();
        }


        private IDaisyAstNode ParseGroup(StatementNode head)
        {
            Consume(TokenKind.StartGroup);
            var groupInner = ParseExpression();
            Consume(TokenKind.EndGroup);
            if (head == null || head.Text == null)
                return groupInner;
            return new GroupOperatorNode(head.With(x => head.Text),
                groupInner);
        }

        private IDaisyAstNode ParsePredicate()
        {
            if(IsOn(TokenKind.EOL) && LookAhead(1,TokenKind.StartGroup)) //Anonymous group
            {
                MoveNextGuaranteed();
                return ParseGroup(null);
            }
            var statement =  ParseStatement();
            if(IsOn(TokenKind.EOL) && LookAhead(1,TokenKind.StartGroup)) //Named group
            {
                MoveNextGuaranteed();
                return ParseGroup(statement);
            }
            AssertHasType(TokenKind.EOL);
            tokenStream.MoveNext();
            return statement;
        }

        private OrOperatorNode ParseOr(IDaisyAstNode left)
        {
            Consume(TokenKind.Or);
            var right = ParseUnaryExpr();
            return new OrOperatorNode(left, right);
        }

        private StatementNode ParseStatement()
        {
            AssertHasType(TokenKind.Statement);
            var statement =  new StatementNode(tokenStream.Current.Value);
            tokenStream.MoveNext();
            return statement;
        }

        private void AssertHasType(TokenKind kind)
        {
            if (tokenStream.Current.Kind != kind)
                throw new UnexpectedTokenException(tokenStream.Current, kind) { };
        }

        private void Consume(TokenKind kind)
        {
            AssertHasType(kind);
            MoveNextGuaranteed();
        }

        private void MoveNextGuaranteed()
        {
            if(!tokenStream.MoveNext()) { throw new UnexpectedEndOfStreamException(); }
        }

        private bool IsOn(params TokenKind[] kinds)
        {
            return IsOn((IEnumerable<TokenKind>)kinds);
        }

        private bool IsOn(IEnumerable<TokenKind> kinds)
        {
            return !tokenStream.IsEOF && kinds.Any(x => tokenStream.Current.Kind == x);
        }

        private bool LookAhead(int lookTo, TokenKind expected)
        {
            var ll = tokenStream.LookAhead(lookTo);
            if (!ll.HasCharactersUpTo) return false;
            return ll.Value.Kind == expected;
        }

        public static DaisyAst Parse(string code)
        {
            var llstream = new LookAheadStream<Token>(new Lexer(code.ToStream()).Lex());
            var parser = new DaisyParser(llstream);
            return parser.Parse();
        }
    }
}
