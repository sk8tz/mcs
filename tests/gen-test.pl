#!/usr/bin/perl -w

my $gmcs = "mono ../gmcs/gmcs.exe";
my $monodis = "monodis";
my $mono = "mono";

my @normal = qw[gen-1 gen-2 gen-3 gen-4 gen-5 gen-6 gen-7 gen-8 gen-9 gen-10 gen-11 gen-12
		gen-14 gen-15 gen-16 gen-18 gen-19 gen-20 gen-21 gen-22 gen-23 gen-24 gen-25
		gen-26 gen-27 gen-28 gen-29 gen-30];
my @library = qw[gen-13 gen-17 gen-31];

sub RunTest
{
    my ($quiet,@args) = @_;
    my $cmdline = join ' ', @args;

    $cmdline .= " > /dev/null" if $quiet;

    print STDERR "Running $cmdline\n";

    my $exitcode = system $cmdline;
    if ($exitcode != 0) {
	print STDERR "Command failed!\n";
	return 0;
    }

    return 1;
}

sub NormalTest
{
    my ($file) = @_;

    my $cs = qq[$file.cs];
    my $exe = qq[$file.exe];

    RunTest (0, $gmcs, $cs) or return 0;
    RunTest (1, $monodis, $exe) or return 0;
    RunTest (0, $mono, $exe) or return 0;

    return 1;
}

sub LibraryTest
{
    my ($file) = @_;

    my $cs_dll = qq[$file-dll.cs];
    my $dll = qq[$file-dll.dll];
    my $cs_exe = qq[$file-exe.cs];
    my $exe = qq[$file-exe.exe];

    RunTest (0, $gmcs, "/target:library", $cs_dll) or return 0;
    RunTest (1, $monodis, $dll) or return 0;
    RunTest (0, $gmcs, "/r:$dll", $cs_exe) or return 0;
    RunTest (1, $monodis, $exe) or return 0;
    RunTest (0, $mono, $exe) or return 0;
}

foreach my $file (@normal) {
    print STDERR "RUNNING TEST: $file\n";
    if (NormalTest ($file)) {
	print STDERR "TEST SUCCEEDED: $file\n";
    } else {
	print STDERR "TEST FAILED: $file\n";
    }
}

foreach my $file (@library) {
    print STDERR "RUNNING LIBRARY TEST: $file\n";
    if (LibraryTest ($file)) {
	print STDERR "TEST SUCCEEDED: $file\n";
    } else {
	print STDERR "TEST FAILED: $file\n";
    }
}
