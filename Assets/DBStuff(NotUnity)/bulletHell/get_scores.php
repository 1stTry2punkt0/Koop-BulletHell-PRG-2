<?php
header("Content-Type: application/json");

$limit = isset($_GET["limit"]) ? intval($_GET["limit"]) : 4;

try {
    $pdo = new PDO("mysql:host=localhost;dbname=db_highscoresbullethell;charset=utf8", "root", "");
    $stmt = $pdo->prepare("SELECT name, score FROM highscores ORDER BY score DESC LIMIT ?");
    $stmt->bindValue(1, $limit, PDO::PARAM_INT);
    $stmt->execute();

    $rows = $stmt->fetchAll(PDO::FETCH_ASSOC);

    echo json_encode(["ok" => true, "scores" => $rows]);
} catch (Exception $e) {
    echo json_encode(["ok" => false, "error" => $e->getMessage()]);
}
