#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using FluentAssertions;
using Xunit;

#pragma warning disable CS0618 // MediaInfoWrapper is obsolete on .NET; it is what these tests are about.

namespace MediaInfo.Wrapper.Tests
{
  public class LegacyWrapperRobustnessTests
  {
    [Fact]
    public void Stream_ThatCannotSeek_DoesNotThrowFromTheConstructor()
    {
      using var stream = new ForwardOnlyStream(new byte[2048]);

      Action create = () => new MediaInfoWrapper(stream);

      create.Should().NotThrow();
    }

    private sealed class ForwardOnlyStream(byte[] content) : Stream
    {
      private readonly MemoryStream _inner = new(content);

      public override bool CanRead => true;

      public override bool CanSeek => false;

      public override bool CanWrite => false;

      public override long Length => throw new NotSupportedException();

      public override long Position
      {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
      }

      public override void Flush()
      {
      }

      public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

      public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

      public override void SetLength(long value) => throw new NotSupportedException();

      public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
  }
}
