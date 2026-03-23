import AppShell from "@/components/layout/AppShell";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ja">
      <body style={{ margin: 0 }}>
        <AppShell>{children}</AppShell>
      </body>
    </html>
  );
}
